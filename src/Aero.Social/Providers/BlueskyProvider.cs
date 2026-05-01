using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aero.Core;
using Aero.Core.Railway;
using Aero.Social.Abstractions;
using Aero.Social.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Providers;

/// <summary>
/// Provides integration with Bluesky (AT Protocol).
/// </summary>
/// <param name="client">The HTTP client to use for API requests.</param>
/// <param name="configuration">The application configuration.</param>
/// <param name="logger">The logger for capturing provider-specific logs.</param>
public class BlueskyProvider(
    HttpClient client,
    IConfiguration configuration,
    ILogger<BlueskyProvider> logger)
    : SocialProviderBase(client, logger)
{
    private readonly IConfiguration _configuration = configuration;

    /// <inheritdoc />
    public override string Identifier => "bluesky";

    /// <inheritdoc />
    public override string Name => "Bluesky";

    /// <inheritdoc />
    public override string[] Scopes => ["write:statuses", "profile", "write:media"];

    /// <inheritdoc />
    public override int MaxConcurrentJobs => 2;

    /// <inheritdoc />
    public override string? Tooltip => "We don't currently support two-factor authentication. If it's enabled on Bluesky, you'll need to disable it.";

    /// <inheritdoc />
    public override int MaxLength(object? additionalSettings = null) => 300;

    /// <inheritdoc />
    public override async Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(6);
        return await Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(new GenerateAuthUrlResponse
        {
            Url = "",
            CodeVerifier = MakeId(10),
            State = state
        });
    }

    /// <inheritdoc />
    public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bodyBytes = Convert.FromBase64String(parameters.Code);
            var bodyJson = Encoding.UTF8.GetString(bodyBytes);
            var authBody = JsonSerializer.Deserialize<BlueskyAuthBody>(bodyJson);

            if (authBody == null)
            {
                return AeroError.HttpRequestError(System.Net.HttpStatusCode.BadRequest, "Invalid auth body");
            }

            return await LoginAsync(authBody.Service, authBody.Identifier, authBody.Password, cancellationToken)
                .BindAsync<BlueskySession, AeroError, AuthTokenDetails>(async session =>
                {
                    return await GetProfileAsync(authBody.Service, session.Did, session.AccessJwt, cancellationToken)
                        .MapAsync<BlueskyProfile, AeroError, AuthTokenDetails>(profile => new AuthTokenDetails
                        {
                            RefreshToken = session.RefreshJwt,
                            ExpiresIn = (int)TimeSpan.FromDays(100).TotalSeconds,
                            AccessToken = session.AccessJwt,
                            Id = session.Did,
                            Name = profile.DisplayName ?? session.Handle,
                            Picture = profile.Avatar ?? string.Empty,
                            Username = session.Handle
                        });
                });
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to authenticate with Bluesky");
            return AeroError.CreateError("Failed to parse authentication parameters");
        }
    }

    /// <inheritdoc />
    public override Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<AuthTokenDetails, AeroError>>(new AuthTokenDetails
        {
            RefreshToken = string.Empty,
            ExpiresIn = 0,
            AccessToken = string.Empty,
            Id = string.Empty,
            Name = string.Empty,
            Picture = string.Empty,
            Username = string.Empty
        });
    }

    /// <inheritdoc />
    public override async Task<Result<PostResponse[], AeroError>> PostAsync(
        string id,
        string accessToken,
        List<PostDetails> posts,
        Integration integration,
        CancellationToken cancellationToken = default)
    {
        if (posts.Count == 0)
        {
            return Array.Empty<PostResponse>();
        }

        var authBody = GetAuthBody(integration);
        if (authBody == null)
        {
            return AeroError.ConfigurationError("No custom instance details for Bluesky");
        }

        return await LoginAsync(authBody.Service, authBody.Identifier, authBody.Password, cancellationToken)
            .BindAsync<BlueskySession, AeroError, PostResponse[]>(async session =>
            {
                var firstPost = posts[0];
                return await UploadMediaForPostAsync(authBody.Service, session, firstPost, cancellationToken)
                    .BindAsync<object?, AeroError, PostResponse[]>(async embed =>
                    {
                        var record = new Dictionary<string, object>
                        {
                            ["$type"] = "app.bsky.feed.post",
                            ["text"] = firstPost.Message,
                            ["createdAt"] = DateTime.UtcNow.ToString("o")
                        };

                        return await DetectFacetsAsync(authBody.Service, session, firstPost.Message, cancellationToken)
                            .BindAsync<List<BlueskyFacet>?, AeroError, PostResponse[]>(async facets =>
                            {
                                if (facets != null && facets.Count > 0)
                                {
                                    record["facets"] = facets;
                                }

                                if (embed != null)
                                {
                                    record["embed"] = embed;
                                }

                                return await CreateRecordAsync(authBody.Service, session, "app.bsky.feed.post", record, cancellationToken)
                                    .MapAsync<BlueskyCreateRecordResponse, AeroError, PostResponse[]>(res =>
                                    {
                                        var postId = res.Uri;
                                        var postKey = postId.Split('/').Last();
                                        return new[]
                                        {
                                            new PostResponse
                                            {
                                                Id = firstPost.Id,
                                                PostId = postId,
                                                Status = "completed",
                                                ReleaseUrl = $"https://bsky.app/profile/{id}/post/{postKey}"
                                            }
                                        };
                                    });
                            });
                    });
            });
    }

    /// <inheritdoc />
    public override async Task<Result<PostResponse[]?, AeroError>> CommentAsync(
        string id,
        string postId,
        string? lastCommentId,
        string accessToken,
        List<PostDetails> posts,
        Integration integration,
        CancellationToken cancellationToken = default)
    {
        if (posts.Count == 0)
        {
            return Array.Empty<PostResponse>();
        }

        var authBody = GetAuthBody(integration);
        if (authBody == null)
        {
            return AeroError.ConfigurationError("No custom instance details for Bluesky");
        }

        return await LoginAsync(authBody.Service, authBody.Identifier, authBody.Password, cancellationToken)
            .BindAsync<BlueskySession, AeroError, PostResponse[]?>(async session =>
            {
                var commentPost = posts[0];
                var parentUri = lastCommentId ?? postId;

                return await GetPostThreadAsync(authBody.Service, session, parentUri, cancellationToken)
                    .BindAsync<BlueskyPostThread?, AeroError, PostResponse[]?>(async parentThread =>
                    {
                        var parentCid = parentThread?.Post?.Cid;
                        var rootUri = parentThread?.Post?.Record?.Reply?.Root?.Uri ?? postId;
                        var rootCid = parentThread?.Post?.Record?.Reply?.Root?.Cid ?? parentCid;

                        return await UploadMediaForPostAsync(authBody.Service, session, commentPost, cancellationToken)
                            .BindAsync<object?, AeroError, PostResponse[]?>(async embed =>
                            {
                                var record = new Dictionary<string, object>
                                {
                                    ["$type"] = "app.bsky.feed.post",
                                    ["text"] = commentPost.Message,
                                    ["createdAt"] = DateTime.UtcNow.ToString("o"),
                                    ["reply"] = new
                                    {
                                        root = new { uri = rootUri, cid = rootCid },
                                        parent = new { uri = parentUri, cid = parentCid }
                                    }
                                };

                                return await DetectFacetsAsync(authBody.Service, session, commentPost.Message, cancellationToken)
                                    .BindAsync<List<BlueskyFacet>?, AeroError, PostResponse[]?>(async facets =>
                                    {
                                        if (facets != null && facets.Count > 0)
                                        {
                                            record["facets"] = facets;
                                        }

                                        if (embed != null)
                                        {
                                            record["embed"] = embed;
                                        }

                                        return await CreateRecordAsync(authBody.Service, session, "app.bsky.feed.post", record, cancellationToken)
                                            .MapAsync<BlueskyCreateRecordResponse, AeroError, PostResponse[]?>(res =>
                                            {
                                                var newPostId = res.Uri;
                                                var postKey = newPostId.Split('/').Last();
                                                return (PostResponse[]?)new[]
                                                {
                                                    new PostResponse
                                                    {
                                                        Id = commentPost.Id,
                                                        PostId = newPostId,
                                                        Status = "completed",
                                                        ReleaseUrl = $"https://bsky.app/profile/{id}/post/{postKey}"
                                                    }
                                                };
                                            });
                                    });
                            });
                    });
            });
    }

    private async Task<Result<BlueskySession, AeroError>> LoginAsync(
        string service,
        string identifier,
        string password,
        CancellationToken cancellationToken)
    {
        var url = $"{service}/xrpc/com.atproto.server.createSession";

        var payload = new
        {
            identifier,
            password
        };

        var request = CreateRequest(url, HttpMethod.Post, payload);
        return await SendRequestAsync<BlueskySession>(request, cancellationToken);
    }

    private async Task<Result<BlueskyProfile, AeroError>> GetProfileAsync(
        string service,
        string actor,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var url = $"{service}/xrpc/app.bsky.actor.getProfile?actor={actor}";

        var request = CreateRequest(url, HttpMethod.Get);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");

        return await SendRequestAsync<BlueskyProfile>(request, cancellationToken);
    }

    private async Task<Result<object?, AeroError>> UploadMediaForPostAsync(
        string service,
        BlueskySession session,
        PostDetails post,
        CancellationToken cancellationToken)
    {
        if (post.Media == null || post.Media.Count == 0)
        {
            return (object?)null;
        }

        var imageMedia = post.Media.Where(m => !m.Path.Contains(".mp4", StringComparison.OrdinalIgnoreCase)).ToList();
        var videoMedia = post.Media.Where(m => m.Path.Contains(".mp4", StringComparison.OrdinalIgnoreCase)).ToList();

        var images = new List<BlueskyUploadedImage>();

        foreach (var media in imageMedia)
        {
            var result = await ReduceImageBySizeAsync(media.Path, cancellationToken)
                .BindAsync<(int Width, int Height, byte[] Buffer), AeroError, BlueskyUploadedImage>(async val =>
                {
                    return await UploadBlobAsync(service, session, val.Buffer, "image/jpeg", cancellationToken)
                        .MapAsync<object, AeroError, BlueskyUploadedImage>(blob => new BlueskyUploadedImage
                        {
                            Width = val.Width,
                            Height = val.Height,
                            Blob = blob
                        });
                });

            if (result is Result<BlueskyUploadedImage, AeroError>.Failure failure)
            {
                return failure.Error;
            }

            images.Add(((Result<BlueskyUploadedImage, AeroError>.Ok)result).Value);
        }

        if (videoMedia.Count > 0)
        {
            return await UploadVideoAsync(service, session, videoMedia[0].Path, cancellationToken)
                .MapAsync<object, AeroError, object?>(v => (object?)v);
        }

        if (images.Count > 0)
        {
            var imagesList = imageMedia.Select((media, index) => new Dictionary<string, object?>
            {
                ["alt"] = media.Alt ?? "",
                ["image"] = images[index].Blob,
                ["aspectRatio"] = new Dictionary<string, int>
                {
                    ["width"] = images[index].Width,
                    ["height"] = images[index].Height
                }
            }).ToList();

            return (Result<object?, AeroError>)new Dictionary<string, object?>
            {
                ["$type"] = "app.bsky.embed.images",
                ["images"] = imagesList
            };
        }

        return (object?)null;
    }

    private async Task<Result<(int Width, int Height, byte[] Buffer), AeroError>> ReduceImageBySizeAsync(
        string url,
        CancellationToken cancellationToken,
        int maxSizeKB = 976)
    {
        return await ReadOrFetchAsync(url, cancellationToken)
            .MapAsync<byte[], AeroError, (int Width, int Height, byte[] Buffer)>(imageBuffer =>
            {
                int width = 800;
                int height = 600;

                while (imageBuffer.Length / 1024 > maxSizeKB)
                {
                    width = (int)(width * 0.9);
                    height = (int)(height * 0.9);

                    if (width < 10 || height < 10)
                    {
                        break;
                    }
                }

                return (width, height, imageBuffer);
            });
    }

    private async Task<Result<object, AeroError>> UploadBlobAsync(
        string service,
        BlueskySession session,
        byte[] data,
        string contentType,
        CancellationToken cancellationToken)
    {
        var url = $"{service}/xrpc/com.atproto.repo.uploadBlob";

        var content = new ByteArrayContent(data);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {session.AccessJwt}");

        return await SendRequestAsync<BlueskyUploadBlobResponse>(request, cancellationToken)
            .BindAsync<BlueskyUploadBlobResponse, AeroError, object>(async result =>
            {
                return result.Blob != null
                    ? (Result<object, AeroError>)result.Blob
                    : AeroError.CreateError("Blob was null after upload");
            });
    }

    private async Task<Result<object, AeroError>> UploadVideoAsync(
        string service,
        BlueskySession session,
        string videoUrl,
        CancellationToken cancellationToken)
    {
        var serviceAuthUrl = $"{service}/xrpc/com.atproto.server.getServiceAuth";
        serviceAuthUrl += $"?aud=did:web:{new Uri(service).Host}";
        serviceAuthUrl += "&lxm=com.atproto.repo.uploadBlob";
        serviceAuthUrl += $"&exp={DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()}";

        var authRequest = CreateRequest(serviceAuthUrl, HttpMethod.Get);
        authRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {session.AccessJwt}");

        return await SendRequestAsync<BlueskyServiceAuthResponse>(authRequest, cancellationToken)
            .BindAsync<BlueskyServiceAuthResponse, AeroError, object>(async authResult =>
            {
                return await ReadOrFetchAsync(videoUrl, cancellationToken)
                    .BindAsync<byte[], AeroError, object>(async videoBytes =>
                    {
                        var uploadUrl = $"https://video.bsky.app/xrpc/app.bsky.video.uploadVideo?did={session.Did}&name=video.mp4";

                        var uploadContent = new ByteArrayContent(videoBytes);
                        uploadContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
                        uploadContent.Headers.ContentLength = videoBytes.Length;

                        var uploadRequest = new HttpRequestMessage(HttpMethod.Post, uploadUrl) { Content = uploadContent };
                        uploadRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {authResult.Token}");

                        return await SendRequestAsync<BlueskyVideoJobStatus>(uploadRequest, cancellationToken)
                            .BindAsync<BlueskyVideoJobStatus, AeroError, object>(async jobStatus =>
                            {
                                var blob = jobStatus.Blob;

                                while (blob == null)
                                {
                                    await Task.Delay(30000, cancellationToken);

                                    var statusUrl = $"https://video.bsky.app/xrpc/app.bsky.video.getJobStatus?jobId={jobStatus.JobId}";
                                    var statusResult = await SendRequestAsync<BlueskyVideoJobStatusResponse>(CreateRequest(statusUrl, HttpMethod.Get), cancellationToken);

                                    if (statusResult is Result<BlueskyVideoJobStatusResponse, AeroError>.Failure statusFailure)
                                    {
                                        return statusFailure.Error;
                                    }

                                    var statusResponseValue = ((Result<BlueskyVideoJobStatusResponse, AeroError>.Ok)statusResult).Value;
                                    blob = statusResponseValue.JobStatus?.Blob;

                                    if (statusResponseValue.JobStatus?.State == "JOB_STATE_FAILED")
                                    {
                                        return AeroError.HttpRequestError(System.Net.HttpStatusCode.BadRequest, "Could not upload video, job failed");
                                    }
                                }

                                return (Result<object, AeroError>)new Dictionary<string, object?>
                                {
                                    ["$type"] = "app.bsky.embed.video",
                                    ["video"] = blob
                                };
                            });
                    });
            });
    }

    private async Task<Result<List<BlueskyFacet>?, AeroError>> DetectFacetsAsync(
        string service,
        BlueskySession session,
        string text,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult<Result<List<BlueskyFacet>?, AeroError>>((List<BlueskyFacet>?)null);
    }

    private async Task<Result<BlueskyCreateRecordResponse, AeroError>> CreateRecordAsync(
        string service,
        BlueskySession session,
        string collection,
        object record,
        CancellationToken cancellationToken)
    {
        var url = $"{service}/xrpc/com.atproto.repo.createRecord";

        var payload = new
        {
            repo = session.Did,
            collection,
            record
        };

        var request = CreateRequest(url, HttpMethod.Post, payload);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {session.AccessJwt}");

        return await SendRequestAsync<BlueskyCreateRecordResponse>(request, cancellationToken);
    }

    private async Task<Result<BlueskyPostThread?, AeroError>> GetPostThreadAsync(
        string service,
        BlueskySession session,
        string uri,
        CancellationToken cancellationToken)
    {
        var url = $"{service}/xrpc/app.bsky.feed.getPostThread?uri={uri}&depth=0";

        var request = CreateRequest(url, HttpMethod.Get);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {session.AccessJwt}");

        return await SendRequestAsync<BlueskyPostThreadResponse>(request, cancellationToken)
            .MapAsync<BlueskyPostThreadResponse, AeroError, BlueskyPostThread?>(result => result?.Thread);
    }

    private static BlueskyAuthBody? GetAuthBody(Integration integration)
    {
        if (string.IsNullOrEmpty(integration.CustomInstanceDetails))
        {
            return null;
        }

        try
        {
            var jsonBytes = Convert.FromBase64String(integration.CustomInstanceDetails);
            var json = Encoding.UTF8.GetString(jsonBytes);
            return JsonSerializer.Deserialize<BlueskyAuthBody>(json);
        }
        catch (Exception ex)
        {
            // Silently fail as per original implementation, or we could log it
            return null;
        }
    }

    #region DTOs

    /// <summary>
    /// Represents the authentication body for Bluesky.
    /// </summary>
    private class BlueskyAuthBody
    {
        /// <summary>
        /// Gets or sets the service URL.
        /// </summary>
        [JsonPropertyName("service")]
        public string Service { get; set; } = "https://bsky.social";

        /// <summary>
        /// Gets or sets the identifier (handle or email).
        /// </summary>
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a Bluesky session.
    /// </summary>
    private class BlueskySession
    {
        /// <summary>
        /// Gets or sets the DID.
        /// </summary>
        [JsonPropertyName("did")]
        public string Did { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the handle.
        /// </summary>
        [JsonPropertyName("handle")]
        public string Handle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the access JWT.
        /// </summary>
        [JsonPropertyName("accessJwt")]
        public string AccessJwt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the refresh JWT.
        /// </summary>
        [JsonPropertyName("refreshJwt")]
        public string RefreshJwt { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a Bluesky profile.
    /// </summary>
    private class BlueskyProfile
    {
        /// <summary>
        /// Gets or sets the DID.
        /// </summary>
        [JsonPropertyName("did")]
        public string Did { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the handle.
        /// </summary>
        [JsonPropertyName("handle")]
        public string Handle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the avatar URL.
        /// </summary>
        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }
    }

    /// <summary>
    /// Represents an uploaded image in Bluesky.
    /// </summary>
    private class BlueskyUploadedImage
    {
        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the blob object.
        /// </summary>
        public object? Blob { get; set; }
    }

    /// <summary>
    /// Represents the response for a blob upload.
    /// </summary>
    private class BlueskyUploadBlobResponse
    {
        /// <summary>
        /// Gets or sets the blob.
        /// </summary>
        [JsonPropertyName("blob")]
        public object? Blob { get; set; }
    }

    /// <summary>
    /// Represents the response for service authentication.
    /// </summary>
    private class BlueskyServiceAuthResponse
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the status of a video job.
    /// </summary>
    private class BlueskyVideoJobStatus
    {
        /// <summary>
        /// Gets or sets the job ID.
        /// </summary>
        [JsonPropertyName("jobId")]
        public string JobId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the blob.
        /// </summary>
        [JsonPropertyName("blob")]
        public object? Blob { get; set; }
    }

    /// <summary>
    /// Represents the response for a video job status.
    /// </summary>
    private class BlueskyVideoJobStatusResponse
    {
        /// <summary>
        /// Gets or sets the job status.
        /// </summary>
        [JsonPropertyName("jobStatus")]
        public BlueskyVideoJobStatusDetail? JobStatus { get; set; }
    }

    /// <summary>
    /// Represents the detail of a video job status.
    /// </summary>
    private class BlueskyVideoJobStatusDetail
    {
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [JsonPropertyName("state")]
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the blob.
        /// </summary>
        [JsonPropertyName("blob")]
        public object? Blob { get; set; }
    }

    /// <summary>
    /// Represents the response for creating a record.
    /// </summary>
    private class BlueskyCreateRecordResponse
    {
        /// <summary>
        /// Gets or sets the URI.
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the CID.
        /// </summary>
        [JsonPropertyName("cid")]
        public string? Cid { get; set; }
    }

    /// <summary>
    /// Represents a facet in a Bluesky post.
    /// </summary>
    private class BlueskyFacet
    {
        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        [JsonPropertyName("index")]
        public BlueskyFacetIndex? Index { get; set; }

        /// <summary>
        /// Gets or sets the features.
        /// </summary>
        [JsonPropertyName("features")]
        public List<object>? Features { get; set; }
    }

    /// <summary>
    /// Represents the index of a facet.
    /// </summary>
    private class BlueskyFacetIndex
    {
        /// <summary>
        /// Gets or sets the byte start.
        /// </summary>
        [JsonPropertyName("byteStart")]
        public int ByteStart { get; set; }

        /// <summary>
        /// Gets or sets the byte end.
        /// </summary>
        [JsonPropertyName("byteEnd")]
        public int ByteEnd { get; set; }
    }

    /// <summary>
    /// Represents the response for getting a post thread.
    /// </summary>
    private class BlueskyPostThreadResponse
    {
        /// <summary>
        /// Gets or sets the thread.
        /// </summary>
        [JsonPropertyName("thread")]
        public BlueskyPostThread? Thread { get; set; }
    }

    /// <summary>
    /// Represents a Bluesky post thread.
    /// </summary>
    private class BlueskyPostThread
    {
        /// <summary>
        /// Gets or sets the post.
        /// </summary>
        [JsonPropertyName("post")]
        public BlueskyPost? Post { get; set; }
    }

    /// <summary>
    /// Represents a Bluesky post.
    /// </summary>
    private class BlueskyPost
    {
        /// <summary>
        /// Gets or sets the URI.
        /// </summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        /// <summary>
        /// Gets or sets the CID.
        /// </summary>
        [JsonPropertyName("cid")]
        public string? Cid { get; set; }

        /// <summary>
        /// Gets or sets the record.
        /// </summary>
        [JsonPropertyName("record")]
        public BlueskyPostRecord? Record { get; set; }
    }

    /// <summary>
    /// Represents the record of a Bluesky post.
    /// </summary>
    private class BlueskyPostRecord
    {
        /// <summary>
        /// Gets or sets the reply reference.
        /// </summary>
        [JsonPropertyName("reply")]
        public BlueskyReplyRef? Reply { get; set; }
    }

    /// <summary>
    /// Represents a reply reference.
    /// </summary>
    private class BlueskyReplyRef
    {
        /// <summary>
        /// Gets or sets the root reference.
        /// </summary>
        [JsonPropertyName("root")]
        public BlueskyStrongRef? Root { get; set; }

        /// <summary>
        /// Gets or sets the parent reference.
        /// </summary>
        [JsonPropertyName("parent")]
        public BlueskyStrongRef? Parent { get; set; }
    }

    /// <summary>
    /// Represents a strong reference in Bluesky.
    /// </summary>
    private class BlueskyStrongRef
    {
        /// <summary>
        /// Gets or sets the URI.
        /// </summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        /// <summary>
        /// Gets or sets the CID.
        /// </summary>
        [JsonPropertyName("cid")]
        public string? Cid { get; set; }
    }

    #endregion
}

