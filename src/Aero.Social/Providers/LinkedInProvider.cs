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
/// Provides integration with LinkedIn for authenticating users and posting content.
/// </summary>
public class LinkedInProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<LinkedInProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    private readonly IConfiguration _configuration = configuration;

    /// <inheritdoc/>
    public override string Identifier => "linkedin";

    /// <inheritdoc/>
    public override string Name => "LinkedIn";

    /// <inheritdoc/>
    public override string[] Scopes => new[]
    {
        "openid",
        "profile",
        "w_member_social",
        "r_basicprofile",
        "rw_organization_admin",
        "w_organization_social",
        "r_organization_social"
    };

    /// <inheritdoc/>
    public override bool OneTimeToken => true;

    /// <inheritdoc/>
    public override bool RefreshWait => true;

    /// <inheritdoc/>
    public override int MaxConcurrentJobs => 2;

    /// <inheritdoc/>
    public override int MaxLength(object? additionalSettings = null) => 3000;

    /// <inheritdoc/>
    protected override ErrorHandlingResult? HandleErrors(string responseBody)
    {
        if (responseBody.Contains("Unable to obtain activity"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.Retry, "Unable to obtain activity");
        }
        return null;
    }

    /// <inheritdoc/>
    public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(6);
        var codeVerifier = MakeId(30);
        var clientId = GetClientId();
        var frontendUrl = GetFrontendUrl();

        var url = $"https://www.linkedin.com/oauth/v2/authorization" +
                  $"?response_type=code" +
                  $"&client_id={clientId}" +
                  $"&prompt=none" +
                  $"&redirect_uri={Uri.EscapeDataString($"{frontendUrl}/integrations/social/linkedin")}" +
                  $"&state={state}" +
                  $"&scope={Uri.EscapeDataString(string.Join(" ", Scopes))}";

        return Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(new GenerateAuthUrlResponse
        {
            Url = url,
            CodeVerifier = codeVerifier,
            State = state
        });
    }

    /// <inheritdoc/>
    public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var clientId = GetClientId();
        var clientSecret = GetClientSecret();
        var frontendUrl = GetFrontendUrl();
        var redirectUri = $"{frontendUrl}/integrations/social/linkedin";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = parameters.Code,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken")
        {
            Content = content
        };

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync<HttpResponseMessage, AeroError, LinkedInTokenResponse>(async response => 
                await DeserializeAsync<LinkedInTokenResponse>(response, cancellationToken))
            .BindAsync<LinkedInTokenResponse, AeroError, AuthTokenDetails>(async tokenResponse =>
            {
                var scopeCheck = CheckScopes(Scopes, tokenResponse.Scope);
                if (scopeCheck is Result<NoneType, AeroError>.Failure failure)
                {
                    return failure.Error;
                }

                return await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken)
                    .BindAsync<LinkedInUserInfo, AeroError, AuthTokenDetails>(async userInfo =>
                    {
                        return await GetVanityNameAsync(tokenResponse.AccessToken, cancellationToken)
                            .MapAsync<string, AeroError, AuthTokenDetails>(vanityName => new AuthTokenDetails
                            {
                                Id = userInfo.Sub,
                                AccessToken = tokenResponse.AccessToken,
                                RefreshToken = tokenResponse.RefreshToken,
                                ExpiresIn = tokenResponse.ExpiresIn,
                                Name = userInfo.Name,
                                Picture = userInfo.Picture ?? string.Empty,
                                Username = vanityName
                            });
                    });
            });
    }

    /// <inheritdoc/>
    public override async Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var clientId = GetClientId();
        var clientSecret = GetClientSecret();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken")
        {
            Content = content
        };

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync<HttpResponseMessage, AeroError, LinkedInTokenResponse>(async response => 
                await DeserializeAsync<LinkedInTokenResponse>(response, cancellationToken))
            .BindAsync<LinkedInTokenResponse, AeroError, AuthTokenDetails>(async tokenResponse =>
            {
                return await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken)
                    .BindAsync<LinkedInUserInfo, AeroError, AuthTokenDetails>(async userInfo =>
                    {
                        return await GetVanityNameAsync(tokenResponse.AccessToken, cancellationToken)
                            .MapAsync<string, AeroError, AuthTokenDetails>(vanityName => new AuthTokenDetails
                            {
                                Id = userInfo.Sub,
                                AccessToken = tokenResponse.AccessToken,
                                RefreshToken = tokenResponse.RefreshToken,
                                ExpiresIn = tokenResponse.ExpiresIn,
                                Name = userInfo.Name,
                                Picture = userInfo.Picture ?? string.Empty,
                                Username = vanityName
                            });
                    });
            });
    }

    /// <inheritdoc/>
    public override async Task<Result<PostResponse[], AeroError>> PostAsync(
        string id,
        string accessToken,
        List<PostDetails> posts,
        Integration integration,
        CancellationToken cancellationToken = default)
    {
        var firstPost = posts.First();
        var settings = firstPost.Settings ?? new Dictionary<string, object>();

        List<string> mediaIds = new();

        if (firstPost.Media != null && firstPost.Media.Count > 0)
        {
            foreach (var media in firstPost.Media)
            {
                var uploadResult = await UploadMediaAsync(id, accessToken, media, cancellationToken);
                if (uploadResult is Result<string, AeroError>.Failure failure) return failure.Error;
                mediaIds.Add(((Result<string, AeroError>.Ok)uploadResult).Value);
            }
        }

        var author = $"urn:li:person:{id}";
        var message = FixText(firstPost.Message);

        var payload = new Dictionary<string, object>
        {
            ["author"] = author,
            ["commentary"] = message,
            ["visibility"] = "PUBLIC",
            ["distribution"] = new
            {
                feedDistribution = "MAIN_FEED",
                targetEntities = Array.Empty<string>(),
                thirdPartyDistributionChannels = Array.Empty<string>()
            },
            ["lifecycleState"] = "PUBLISHED",
            ["isReshareDisabledByAuthor"] = false
        };

        if (mediaIds.Count > 0)
        {
            if (mediaIds.Count == 1)
            {
                payload["content"] = new { media = new { id = mediaIds[0] } };
            }
            else
            {
                payload["content"] = new
                {
                    multiImage = new
                    {
                        images = mediaIds.Select(m => new { id = m }).ToArray()
                    }
                };
            }
        }

        var request = CreateRequest("https://api.linkedin.com/rest/posts", HttpMethod.Post, payload);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("LinkedIn-Version", "202511");
        request.Headers.Add("X-Restli-Protocol-Version", "2.0.0");

        return await SendRequestAsync(request, cancellationToken)
            .MapAsync<HttpResponseMessage, AeroError, PostResponse[]>(response =>
            {
                var postId = response.Headers.TryGetValues("x-restli-id", out var values) ? values.FirstOrDefault() ?? string.Empty : string.Empty;

                return new[]
                {
                    new PostResponse
                    {
                        Id = firstPost.Id,
                        PostId = postId,
                        ReleaseUrl = $"https://www.linkedin.com/feed/update/{postId}",
                        Status = "posted"
                    }
                };
            });
    }

    /// <inheritdoc/>
    public override async Task<Result<PostResponse[]?, AeroError>> CommentAsync(
        string id,
        string postId,
        string? lastCommentId,
        string accessToken,
        List<PostDetails> posts,
        Integration integration,
        CancellationToken cancellationToken = default)
    {
        var commentPost = posts.First();
        var actor = $"urn:li:person:{id}";

        var payload = new
        {
            actor,
            @object = postId,
            message = new { text = FixText(commentPost.Message) }
        };

        var request = CreateRequest($"https://api.linkedin.com/v2/socialActions/{Uri.EscapeDataString(postId)}/comments", HttpMethod.Post, payload);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync<HttpResponseMessage, AeroError, LinkedInCommentResponse>(async response => 
                await DeserializeAsync<LinkedInCommentResponse>(response, cancellationToken))
            .MapAsync<LinkedInCommentResponse, AeroError, PostResponse[]?>(commentResponse => new[]
            {
                new PostResponse
                {
                    Id = commentPost.Id,
                    PostId = commentResponse.Object,
                    ReleaseUrl = $"https://www.linkedin.com/embed/feed/update/{commentResponse.Object}",
                    Status = "posted"
                }
            });
    }

    /// <summary>
    /// Uploads media content to LinkedIn.
    /// </summary>
    /// <param name="personId">The LinkedIn person ID.</param>
    /// <param name="accessToken">The LinkedIn access token.</param>
    /// <param name="media">The media content to upload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media ID if successful; otherwise, an error.</returns>
    private async Task<Result<string, AeroError>> UploadMediaAsync(string personId, string accessToken, MediaContent media, CancellationToken cancellationToken)
    {
        var isVideo = media.Path.Contains(".mp4", StringComparison.OrdinalIgnoreCase);
        var endpoint = isVideo ? "videos" : "images";

        return await ReadOrFetchAsync(media.Path, cancellationToken)
            .BindAsync<byte[], AeroError, string>(async mediaBytes => 
            {
                var initializePayload = new
                {
                    initializeUploadRequest = new
                    {
                        owner = $"urn:li:person:{personId}",
                        fileSizeBytes = isVideo ? mediaBytes.Length : (long?)null
                    }
                };

                var request = CreateRequest($"https://api.linkedin.com/rest/{endpoint}?action=initializeUpload", HttpMethod.Post, initializePayload);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
                request.Headers.Add("LinkedIn-Version", "202511");
                request.Headers.Add("X-Restli-Protocol-Version", "2.0.0");

                return await SendRequestAsync(request, cancellationToken)
                    .BindAsync<HttpResponseMessage, AeroError, LinkedInUploadResponse>(async response => 
                        await DeserializeAsync<LinkedInUploadResponse>(response, cancellationToken))
                    .BindAsync<LinkedInUploadResponse, AeroError, string>(async uploadResponse => 
                    {
                        var uploadUrl = uploadResponse.Value.UploadUrl;
                        var resourceId = uploadResponse.Value.Image;

                        var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
                        {
                            Content = new ByteArrayContent(mediaBytes)
                        };
                        uploadRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                        uploadRequest.Headers.Add("X-Restli-Protocol-Version", "2.0.0");
                        uploadRequest.Headers.Add("LinkedIn-Version", "202511");

                        if (isVideo)
                        {
                            uploadRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                        }

                        return (await SendRequestAsync(uploadRequest, cancellationToken))
                            .Map(_ => resourceId);
                    });
            });
    }

    /// <summary>
    /// Retrieves user information from LinkedIn userinfo endpoint.
    /// </summary>
    /// <param name="accessToken">The LinkedIn access token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user information.</returns>
    protected async Task<Result<LinkedInUserInfo, AeroError>> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync<HttpResponseMessage, AeroError, LinkedInUserInfo>(async response => 
                await DeserializeAsync<LinkedInUserInfo>(response, cancellationToken));
    }

    /// <summary>
    /// Retrieves the user's vanity name from LinkedIn profile endpoint.
    /// </summary>
    /// <param name="accessToken">The LinkedIn access token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The vanity name if found; otherwise, an empty string.</returns>
    protected async Task<Result<string, AeroError>> GetVanityNameAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/me");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync<HttpResponseMessage, AeroError, LinkedInMeResponse>(async response => 
                await DeserializeAsync<LinkedInMeResponse>(response, cancellationToken))
            .MapAsync<LinkedInMeResponse, AeroError, string>(meResponse => meResponse.VanityName ?? string.Empty);
    }

    /// <summary>
    /// Escapes special characters for LinkedIn post commentary.
    /// </summary>
    /// <param name="text">The text to fix.</param>
    /// <returns>The fixed text.</returns>
    protected static string FixText(string text)
    {
        var specialChars = new[] { "\\", "<", ">", "#", "~", "_", "|", "[", "]", "*", "(", ")", "{", "}", "@" };
        foreach (var ch in specialChars)
        {
            text = text.Replace(ch, $"\\{ch}");
        }
        return text;
    }

    /// <summary>
    /// Helper to get a typed value from the settings dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="settings">The settings dictionary.</param>
    /// <param name="key">The setting key.</param>
    /// <returns>The typed value or default.</returns>
    protected static T? GetSettingValue<T>(Dictionary<string, object> settings, string key)
    {
        if (!settings.TryGetValue(key, out var value))
            return default;

        if (value is T typedValue)
            return typedValue;

        var json = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<T>(json);
    }

    /// <summary>Gets the LinkedIn client ID from configuration.</summary>
    protected string GetClientId() => _configuration["LINKEDIN_CLIENT_ID"] ?? throw new InvalidOperationException("LINKEDIN_CLIENT_ID not configured");
    /// <summary>Gets the LinkedIn client secret from configuration.</summary>
    protected string GetClientSecret() => _configuration["LINKEDIN_CLIENT_SECRET"] ?? throw new InvalidOperationException("LINKEDIN_CLIENT_SECRET not configured");
    /// <summary>Gets the frontend URL from configuration.</summary>
    protected string GetFrontendUrl() => _configuration["FRONTEND_URL"] ?? throw new InvalidOperationException("FRONTEND_URL not configured");

    //#region DTOs

    /// <summary>Represents the LinkedIn OAuth token response.</summary>
    protected class LinkedInTokenResponse
    {
        /// <summary>Gets or sets the access token.</summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>Gets or sets the refresh token.</summary>
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>Gets or sets the expiration time in seconds.</summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>Gets or sets the granted scopes.</summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }

    /// <summary>Represents user information from LinkedIn.</summary>
    protected class LinkedInUserInfo
    {
        /// <summary>Gets or sets the sub (subject) identifier.</summary>
        [JsonPropertyName("sub")]
        public string Sub { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's profile picture URL.</summary>
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
    }

    /// <summary>Represents the LinkedIn me response details.</summary>
    protected class LinkedInMeResponse
    {
        /// <summary>Gets or sets the user's vanity name.</summary>
        [JsonPropertyName("vanityName")]
        public string? VanityName { get; set; }
    }

    /// <summary>Represents the LinkedIn media upload response.</summary>
    protected class LinkedInUploadResponse
    {
        /// <summary>Gets or sets the upload result value.</summary>
        [JsonPropertyName("value")]
        public LinkedInUploadValue Value { get; set; } = new();
    }

    /// <summary>Represents the LinkedIn media upload details.</summary>
    protected class LinkedInUploadValue
    {
        /// <summary>Gets or sets the upload URL.</summary>
        [JsonPropertyName("uploadUrl")]
        public string UploadUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets the media (image/video) resource ID.</summary>
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;
    }

    /// <summary>Represents a LinkedIn comment response.</summary>
    protected class LinkedInCommentResponse
    {
        /// <summary>Gets or sets the comment object ID.</summary>
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;
    }

    //#endregion
}
