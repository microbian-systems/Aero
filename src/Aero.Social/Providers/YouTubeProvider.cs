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
/// Provides integration with YouTube for video uploading and channel management.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="YouTubeProvider"/> class.
/// </remarks>
/// <param name="httpClient">The HTTP client for API requests.</param>
/// <param name="configuration">The application configuration.</param>
/// <param name="logger">The logger instance.</param>
public class YouTubeProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<YouTubeProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    private readonly IConfiguration _configuration = configuration;

    /// <inheritdoc/>
    public override string Identifier => "youtube";

    /// <inheritdoc/>
    public override string Name => "YouTube";

    /// <inheritdoc/>
    public override bool IsBetweenSteps => true;

    /// <inheritdoc/>
    public override string[] Scopes =>
    [
        "https://www.googleapis.com/auth/userinfo.profile",
        "https://www.googleapis.com/auth/userinfo.email",
        "https://www.googleapis.com/auth/youtube",
        "https://www.googleapis.com/auth/youtube.force-ssl",
        "https://www.googleapis.com/auth/youtube.readonly",
        "https://www.googleapis.com/auth/youtube.upload",
        "https://www.googleapis.com/auth/youtubepartner",
        "https://www.googleapis.com/auth/yt-analytics.readonly"
    ];

    /// <inheritdoc/>
    public override int MaxConcurrentJobs => 200;

    /// <inheritdoc/>
    public override int MaxLength(object? additionalSettings = null) => 5000;

    /// <inheritdoc/>
    protected override ErrorHandlingResult? HandleErrors(string responseBody)
    {
        if (responseBody.Contains("invalidTitle"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "We have uploaded your video but we could not set the title. Title is too long.");
        }

        if (responseBody.Contains("failedPrecondition"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "We have uploaded your video but we could not set the thumbnail. Thumbnail size is too large.");
        }

        if (responseBody.Contains("uploadLimitExceeded"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "You have reached your daily upload limit, please try again tomorrow.");
        }

        if (responseBody.Contains("youtubeSignupRequired"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "You have to link your YouTube account to your Google account first.");
        }

        if (responseBody.Contains("youtube.thumbnail"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Your account is not verified, we have uploaded your video but we could not set the thumbnail. Please verify your account and try again.");
        }

        if (responseBody.Contains("Unauthorized"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.RefreshToken, "Token expired or invalid, please reconnect your YouTube account.");
        }

        if (responseBody.Contains("UNAUTHENTICATED") || responseBody.Contains("invalid_grant"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.RefreshToken, "Please re-authenticate your YouTube account");
        }

        return null;
    }

    /// <inheritdoc/>
    public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(7);
        var frontendUrlResult = GetFrontendUrl();
        var clientIdResult = GetClientId();

        if (clientIdResult is Result<string, AeroError>.Failure clientIdFailure) 
            return Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(clientIdFailure.Error);
        
        if (frontendUrlResult is Result<string, AeroError>.Failure frontendUrlFailure) 
            return Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(frontendUrlFailure.Error);

        var clientId = ((Result<string, AeroError>.Ok)clientIdResult).Value;
        var frontendUrl = ((Result<string, AeroError>.Ok)frontendUrlResult).Value;

        var redirectUri = $"{frontendUrl}/integrations/social/youtube";

        var url = $"https://accounts.google.com/o/oauth2/v2/auth" +
                  $"?client_id={clientId}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&response_type=code" +
                  $"&scope={Uri.EscapeDataString(string.Join(" ", Scopes))}" +
                  $"&access_type=offline" +
                  $"&prompt=consent" +
                  $"&state={state}";

        return Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(new GenerateAuthUrlResponse
        {
            Url = url,
            CodeVerifier = MakeId(11),
            State = state
        });
    }

    /// <inheritdoc/>
    public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var clientIdResult = GetClientId();
        var clientSecretResult = GetClientSecret();
        var frontendUrlResult = GetFrontendUrl();

        if (clientIdResult is Result<string, AeroError>.Failure clientIdFailure) return clientIdFailure.Error;
        if (clientSecretResult is Result<string, AeroError>.Failure clientSecretFailure) return clientSecretFailure.Error;
        if (frontendUrlResult is Result<string, AeroError>.Failure frontendUrlFailure) return frontendUrlFailure.Error;

        var clientId = ((Result<string, AeroError>.Ok)clientIdResult).Value;
        var clientSecret = ((Result<string, AeroError>.Ok)clientSecretResult).Value;
        var frontendUrl = ((Result<string, AeroError>.Ok)frontendUrlResult).Value;

        var redirectUri = $"{frontendUrl}/integrations/social/youtube";

        return await ExchangeCodeForTokenAsync(clientId, clientSecret, redirectUri, parameters.Code, cancellationToken)
            .BindAsync<GoogleTokenResponse, AeroError, AuthTokenDetails>(async tokenResponse =>
            {
                var grantedScopes = tokenResponse.Scope ?? string.Empty;
                var scopeCheck = CheckScopes(Scopes, grantedScopes);
                if (scopeCheck is Result<NoneType, AeroError>.Failure failure)
                {
                    return failure.Error;
                }

                return await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken)
                    .MapAsync<GoogleUserInfo, AeroError, AuthTokenDetails>(userInfo => new AuthTokenDetails
                    {
                        Id = userInfo.Id,
                        Name = userInfo.Name,
                        AccessToken = tokenResponse.AccessToken,
                        RefreshToken = tokenResponse.RefreshToken ?? string.Empty,
                        ExpiresIn = tokenResponse.ExpiresIn ?? 3600,
                        Picture = userInfo.Picture ?? string.Empty,
                        Username = string.Empty
                    });
            });
    }

    /// <inheritdoc/>
    public override async Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var clientIdResult = GetClientId();
        var clientSecretResult = GetClientSecret();

        if (clientIdResult is Result<string, AeroError>.Failure clientIdFailure) return clientIdFailure.Error;
        if (clientSecretResult is Result<string, AeroError>.Failure clientSecretFailure) return clientSecretFailure.Error;

        var clientId = ((Result<string, AeroError>.Ok)clientIdResult).Value;
        var clientSecret = ((Result<string, AeroError>.Ok)clientSecretResult).Value;

        return await RefreshAccessTokenAsync(clientId, clientSecret, refreshToken, cancellationToken)
            .BindAsync<GoogleTokenResponse, AeroError, AuthTokenDetails>(async tokenResponse =>
            {
                return await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken)
                    .MapAsync<GoogleUserInfo, AeroError, AuthTokenDetails>(userInfo => new AuthTokenDetails
                    {
                        Id = userInfo.Id,
                        Name = userInfo.Name,
                        AccessToken = tokenResponse.AccessToken,
                        RefreshToken = tokenResponse.RefreshToken ?? refreshToken,
                        ExpiresIn = tokenResponse.ExpiresIn ?? 3600,
                        Picture = userInfo.Picture ?? string.Empty,
                        Username = string.Empty
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
        var firstPost = posts.FirstOrDefault();
        if (firstPost == null)
        {
            return AeroError.ValidationError(["No posts provided."]);
        }

        var settings = firstPost.Settings ?? new Dictionary<string, object>();

        if (firstPost.Media == null || firstPost.Media.Count == 0)
        {
            return AeroError.ValidationError(["YouTube requires a video attachment"]);
        }

        var video = firstPost.Media[0];
        var title = GetSettingValue<string>(settings, "title") ?? "Untitled";
        var description = firstPost.Message;
        var tags = GetSettingValue<List<string>>(settings, "tags") ?? [];
        var categoryId = GetSettingValue<string>(settings, "category_id") ?? "22";
        var privacyStatus = GetSettingValue<string>(settings, "privacy_status") ?? "public";
        var madeForKids = GetSettingValue<bool?>(settings, "made_for_kids") ?? false;
        var thumbnail = GetSettingValue<string>(settings, "thumbnail_url");

        return await UploadVideoAsync(accessToken, video.Path, title, description, tags, categoryId, privacyStatus, madeForKids, cancellationToken)
            .BindAsync<string, AeroError, PostResponse[]>(async videoId =>
            {
                if (!string.IsNullOrEmpty(thumbnail))
                {
                    var thumbnailResult = await SetThumbnailAsync(accessToken, videoId, thumbnail, cancellationToken);
                    if (thumbnailResult is Result<NoneType, AeroError>.Failure failure)
                    {
                        return (Result<PostResponse[], AeroError>)failure.Error;
                    }
                }

                var videoUrl = $"https://www.youtube.com/watch?v={videoId}";
                return new[]
                {
                    new PostResponse
                    {
                        Id = firstPost.Id,
                        PostId = videoId,
                        ReleaseUrl = videoUrl,
                        Status = "success"
                    }
                };
            });
    }

    /// <summary>
    /// Uploads a video to YouTube.
    /// </summary>
    /// <param name="accessToken">The user access token.</param>
    /// <param name="videoUrl">The URL or path of the video to upload.</param>
    /// <param name="title">The video title.</param>
    /// <param name="description">The video description.</param>
    /// <param name="tags">The video tags.</param>
    /// <param name="categoryId">The YouTube category ID.</param>
    /// <param name="privacyStatus">The video privacy status.</param>
    /// <param name="madeForKids">Whether the video is made for kids.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the uploaded video ID or an error.</returns>
    private async Task<Result<string, AeroError>> UploadVideoAsync(
        string accessToken,
        string videoUrl,
        string title,
        string description,
        List<string> tags,
        string categoryId,
        string privacyStatus,
        bool madeForKids,
        CancellationToken cancellationToken)
    {
        return await ReadOrFetchAsync(videoUrl, cancellationToken)
            .BindAsync<byte[], AeroError, string>(async videoBytes =>
            {
                var metadata = new
                {
                    snippet = new
                    {
                        title,
                        description,
                        tags = tags.Count > 0 ? tags.ToArray() : null,
                        categoryId
                    },
                    status = new
                    {
                        privacyStatus,
                        selfDeclaredMadeForKids = madeForKids
                    }
                };

                var metadataJson = JsonSerializer.Serialize(metadata);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/upload/youtube/v3/videos?uploadType=resumable&part=snippet,status")
                {
                    Content = new StringContent(metadataJson, System.Text.Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                return await SendRequestAsync(request, cancellationToken)
                    .BindAsync<HttpResponseMessage, AeroError, string>(async initResponse =>
                    {
                        var uploadUrl = initResponse.Headers.Location?.ToString();
                        if (string.IsNullOrEmpty(uploadUrl))
                        {
                            return AeroError.HttpRequestError(System.Net.HttpStatusCode.InternalServerError, "Failed to get upload URL from YouTube");
                        }

                        var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
                        {
                            Content = new ByteArrayContent(videoBytes)
                        };
                        uploadRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                        uploadRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/*");

                        return await SendRequestAsync<YouTubeVideoResponse>(uploadRequest, cancellationToken)
                            .MapAsync<YouTubeVideoResponse, AeroError, string>(videoResponse => videoResponse.Id);
                    });
            });
    }

    /// <summary>
    /// Sets the thumbnail for a YouTube video.
    /// </summary>
    /// <param name="accessToken">The user access token.</param>
    /// <param name="videoId">The YouTube video ID.</param>
    /// <param name="thumbnailUrl">The URL or path of the thumbnail image.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or an error.</returns>
    private async Task<Result<NoneType, AeroError>> SetThumbnailAsync(string accessToken, string videoId, string thumbnailUrl, CancellationToken cancellationToken)
    {
        return await ReadOrFetchAsync(thumbnailUrl, cancellationToken)
            .BindAsync<byte[], AeroError, NoneType>(async thumbnailBytes =>
            {
                var content = new MultipartFormDataContent
                {
                    { new ByteArrayContent(thumbnailBytes), "image", "thumbnail.jpg" }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.googleapis.com/upload/youtube/v3/thumbnails/set?videoId={videoId}")
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                return await SendRequestAsync(request, cancellationToken)
                    .MapAsync<HttpResponseMessage, AeroError, NoneType>(_ => new NoneType());
            });
    }

    /// <summary>
    /// Retrieves the list of YouTube channels managed by the authenticated user.
    /// </summary>
    /// <param name="accessToken">The user access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing a list of YouTube channels or an AeroError.</returns>
    public async Task<Result<List<YouTubeChannel>, AeroError>> GetChannelsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/youtube/v3/channels?part=snippet,contentDetails,statistics&mine=true");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        return await SendRequestAsync<YouTubeChannelsResponse>(request, cancellationToken)
            .MapAsync<YouTubeChannelsResponse, AeroError, List<YouTubeChannel>>(channelsResponse => channelsResponse.Items?.Select(c => new YouTubeChannel
            {
                Id = c.Id,
                Name = c.Snippet?.Title ?? "Unnamed Channel",
                Username = c.Snippet?.CustomUrl ?? string.Empty,
                Picture = c.Snippet?.Thumbnails?.Default?.Url ?? string.Empty,
                SubscriberCount = c.Statistics?.SubscriberCount ?? "0"
            }).ToList() ?? []);
    }

    /// <summary>
    /// Exchanges an authorization code for an OAuth token.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <param name="redirectUri">The redirect URI.</param>
    /// <param name="code">The authorization code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the token response or an error.</returns>
    private async Task<Result<GoogleTokenResponse, AeroError>> ExchangeCodeForTokenAsync(string clientId, string clientSecret, string redirectUri, string code, CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = redirectUri
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = content
        };

        return await SendRequestAsync<GoogleTokenResponse>(request, cancellationToken);
    }

    /// <summary>
    /// Refreshes an expired access token.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the refreshed token response or an error.</returns>
    private async Task<Result<GoogleTokenResponse, AeroError>> RefreshAccessTokenAsync(string clientId, string clientSecret, string refreshToken, CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = content
        };

        return await SendRequestAsync<GoogleTokenResponse>(request, cancellationToken);
    }

    /// <summary>
    /// Retrieves Google user information.
    /// </summary>
    /// <param name="accessToken">The user access token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the user information or an error.</returns>
    private async Task<Result<GoogleUserInfo, AeroError>> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        return await SendRequestAsync<GoogleUserInfo>(request, cancellationToken);
    }

    /// <summary>
    /// Retrieves a setting value from the provided settings dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    /// <param name="settings">The settings dictionary.</param>
    /// <param name="key">The setting key.</param>
    /// <returns>The typed value if successful; otherwise, the default value of <typeparamref name="T"/>.</returns>
    private static T? GetSettingValue<T>(Dictionary<string, object> settings, string key)
    {
        if (settings == null || !settings.TryGetValue(key, out var value))
            return default;

        if (value is T typedValue)
            return typedValue;

        try
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    private Result<string, AeroError> GetClientId() => _configuration["YOUTUBE_CLIENT_ID"] is string s ? s : AeroError.ConfigurationError("YOUTUBE_CLIENT_ID not configured");
    private Result<string, AeroError> GetClientSecret() => _configuration["YOUTUBE_CLIENT_SECRET"] is string s ? s : AeroError.ConfigurationError("YOUTUBE_CLIENT_SECRET not configured");
    private Result<string, AeroError> GetFrontendUrl() => _configuration["FRONTEND_URL"] is string s ? s : AeroError.ConfigurationError("FRONTEND_URL not configured");

    #region DTOs

    /// <summary>
    /// Represents the token response from Google OAuth.
    /// </summary>
    private class GoogleTokenResponse
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the expiration time in seconds.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the granted scopes.
        /// </summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }

    /// <summary>
    /// Represents user information from Google.
    /// </summary>
    private class GoogleUserInfo
    {
        /// <summary>
        /// Gets or sets the unique Google ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL of the user's profile picture.
        /// </summary>
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
    }

    /// <summary>
    /// Represents the response from a YouTube video upload.
    /// </summary>
    private class YouTubeVideoResponse
    {
        /// <summary>
        /// Gets or sets the video ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the response containing a list of YouTube channels.
    /// </summary>
    private class YouTubeChannelsResponse
    {
        /// <summary>
        /// Gets or sets the list of channel items.
        /// </summary>
        [JsonPropertyName("items")]
        public List<YouTubeChannelItem>? Items { get; set; }
    }

    /// <summary>
    /// Represents a single YouTube channel item in a list response.
    /// </summary>
    private class YouTubeChannelItem
    {
        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the channel snippet details.
        /// </summary>
        [JsonPropertyName("snippet")]
        public YouTubeSnippet? Snippet { get; set; }

        /// <summary>
        /// Gets or sets the channel statistics.
        /// </summary>
        [JsonPropertyName("statistics")]
        public YouTubeStatistics? Statistics { get; set; }
    }

    /// <summary>
    /// Represents the metadata snippet for a YouTube channel.
    /// </summary>
    private class YouTubeSnippet
    {
        /// <summary>
        /// Gets or sets the channel title.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the custom URL (handle).
        /// </summary>
        [JsonPropertyName("customUrl")]
        public string? CustomUrl { get; set; }

        /// <summary>
        /// Gets or sets the channel thumbnails.
        /// </summary>
        [JsonPropertyName("thumbnails")]
        public YouTubeThumbnails? Thumbnails { get; set; }
    }

    /// <summary>
    /// Represents the various sizes of thumbnails for a channel.
    /// </summary>
    private class YouTubeThumbnails
    {
        /// <summary>
        /// Gets or sets the default thumbnail.
        /// </summary>
        [JsonPropertyName("default")]
        public YouTubeThumbnail? Default { get; set; }
    }

    /// <summary>
    /// Represents a single YouTube thumbnail image details.
    /// </summary>
    private class YouTubeThumbnail
    {
        /// <summary>
        /// Gets or sets the thumbnail image URL.
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    /// <summary>
    /// Represents statistics for a YouTube channel.
    /// </summary>
    private class YouTubeStatistics
    {
        /// <summary>
        /// Gets or sets the subscriber count as a string.
        /// </summary>
        [JsonPropertyName("subscriberCount")]
        public string? SubscriberCount { get; set; }
    }

    /// <summary>
    /// Represents a YouTube channel's basic information.
    /// </summary>
    public class YouTubeChannel
    {
        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the channel name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the custom handle/URL of the channel.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL of the channel's profile picture.
        /// </summary>
        public string Picture { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the subscriber count as a string.
        /// </summary>
        public string SubscriberCount { get; set; } = string.Empty;
    }

    #endregion
}
