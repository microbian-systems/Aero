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
/// Provides integration with Facebook Pages for posting and analytics.
/// </summary>
public class FacebookProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<FacebookProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    private const string GraphApiBaseUrl = "https://graph.facebook.com/v20.0";

    /// <inheritdoc/>
    public override string Identifier => "facebook";

    /// <inheritdoc/>
    public override string Name => "Facebook Page";

    /// <inheritdoc/>
    public override bool IsBetweenSteps => true;

    /// <inheritdoc/>
    public override string[] Scopes =>
    [
        "pages_show_list",
        "business_management",
        "pages_manage_posts",
        "pages_manage_engagement",
        "pages_read_engagement",
        "read_insights"
    ];

    /// <inheritdoc/>
    public override int MaxConcurrentJobs => 100;

    /// <inheritdoc/>
    public override int MaxLength(object? additionalSettings = null) => 63206;

    /// <inheritdoc/>
    protected override ErrorHandlingResult? HandleErrors(string responseBody)
    {
        if (responseBody.Contains("Error validating access token"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.RefreshToken, "Please re-authenticate your Facebook account");
        }

        if (responseBody.Contains("490") || responseBody.Contains("REVOKED_ACCESS_TOKEN"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.RefreshToken, "Access token expired, please re-authenticate");
        }

        if (responseBody.Contains("1366046"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Photos should be smaller than 4 MB and saved as JPG, PNG");
        }

        if (responseBody.Contains("1390008"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "You are posting too fast, please slow down");
        }

        if (responseBody.Contains("1346003"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Content flagged as abusive by Facebook");
        }

        if (responseBody.Contains("1404078"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.RefreshToken, "Page publishing authorization required, please re-authenticate");
        }

        return null;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override async Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var appId = configuration["FACEBOOK_APP_ID"];
        var frontendUrl = configuration["FRONTEND_URL"];

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(frontendUrl))
        {
            return AeroError.CreateError("Facebook configuration missing (FACEBOOK_APP_ID or FRONTEND_URL)");
        }

        var state = MakeId(6);
        var url = $"https://www.facebook.com/v20.0/dialog/oauth" +
                  $"?client_id={appId}" +
                  $"&redirect_uri={Uri.EscapeDataString($"{frontendUrl}/integrations/social/facebook")}" +
                  $"&state={state}" +
                  $"&scope={string.Join(",", Scopes)}";

        return new GenerateAuthUrlResponse
        {
            Url = url,
            CodeVerifier = MakeId(10),
            State = state
        };
    }

    /// <inheritdoc/>
    public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var appId = configuration["FACEBOOK_APP_ID"];
        var appSecret = configuration["FACEBOOK_APP_SECRET"];
        var frontendUrl = configuration["FRONTEND_URL"];

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret) || string.IsNullOrEmpty(frontendUrl))
        {
            return AeroError.CreateError("Facebook configuration missing (FACEBOOK_APP_ID, FACEBOOK_APP_SECRET, or FRONTEND_URL)");
        }

        var redirectUri = $"{frontendUrl}/integrations/social/facebook";

        return await ExchangeCodeForTokenAsync(appId, appSecret, redirectUri, parameters.Code, cancellationToken)
            .BindAsync(shortLivedToken => ExchangeForLongLivedTokenAsync(appId, appSecret, shortLivedToken, cancellationToken))
            .BindAsync(async longLivedToken =>
            {
                return await GetPermissionsAsync(longLivedToken, cancellationToken)
                    .BindAsync(async permissions =>
                    {
                        var scopeCheck = CheckScopes(Scopes, permissions);
                        if (scopeCheck is Result<NoneType, AeroError>.Failure failure)
                        {
                            return failure.Error;
                        }

                        return await GetUserInfoAsync(longLivedToken, cancellationToken)
                            .MapAsync(async userInfo => new AuthTokenDetails
                            {
                                Id = userInfo.Id,
                                Name = userInfo.Name,
                                AccessToken = longLivedToken,
                                RefreshToken = longLivedToken,
                                ExpiresIn = (int)TimeSpan.FromDays(59).TotalSeconds,
                                Picture = userInfo.Picture?.Data?.Url ?? string.Empty,
                                Username = string.Empty
                            });
                    });
            });
    }

    /// <inheritdoc/>
    public override async Task<Result<AuthTokenDetails?, AeroError>> ReConnectAsync(
        string id,
        string requiredId,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        return await FetchPageInformationAsync(accessToken, new { page = requiredId }, cancellationToken)
            .MapAsync(async page => (AuthTokenDetails?)new AuthTokenDetails
            {
                Id = page!.Id,
                Name = page.Name,
                AccessToken = page.AccessToken,
                Picture = page.Picture,
                Username = page.Username
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
        var linkUrl = GetSettingValue<string>(settings, "url");

        if (firstPost.Media != null && firstPost.Media.Count > 0)
        {
            var media = firstPost.Media[0];
            if (media.Path.Contains(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                return await PostVideoAsync(id, accessToken, firstPost, cancellationToken);
            }
        }

        return await PostFeedAsync(id, accessToken, firstPost, linkUrl, cancellationToken);
    }

    private async Task<Result<PostResponse[], AeroError>> PostFeedAsync(
        string pageId,
        string accessToken,
        PostDetails post,
        string? linkUrl,
        CancellationToken cancellationToken)
    {
        var uploadedPhotoIds = new List<string>();

        if (post.Media != null && post.Media.Count > 0)
        {
            foreach (var media in post.Media)
            {
                var photoResult = await UploadPhotoAsync(pageId, accessToken, media.Path, false, cancellationToken);
                if (photoResult is Result<string, AeroError>.Ok ok)
                {
                    uploadedPhotoIds.Add(ok.Value);
                }
                else
                {
                    return ((Result<string, AeroError>.Failure)photoResult).Error;
                }
            }
        }

        var payload = new Dictionary<string, object?>
        {
            ["message"] = post.Message,
            ["published"] = true
        };

        if (uploadedPhotoIds.Count > 0)
        {
            payload["attached_media"] = uploadedPhotoIds.Select(id => new { media_fbid = id }).ToArray();
        }

        if (!string.IsNullOrEmpty(linkUrl))
        {
            payload["link"] = linkUrl;
        }

        var url = $"{GraphApiBaseUrl}/{pageId}/feed?access_token={accessToken}&fields=id,permalink_url";
        var request = CreateRequest(url, HttpMethod.Post, payload);
        
        return await SendRequestAsync(request, cancellationToken)
            .BindAsync(async response =>
            {
                var postResponse = await DeserializeAsync<FacebookPostResponse>(response, cancellationToken);
                return new[]
                {
                    new PostResponse
                    {
                        Id = post.Id,
                        PostId = postResponse.Id,
                        ReleaseUrl = postResponse.PermalinkUrl ?? string.Empty,
                        Status = "success"
                    }
                };
            });
    }

    private async Task<Result<PostResponse[], AeroError>> PostVideoAsync(
        string pageId,
        string accessToken,
        PostDetails post,
        CancellationToken cancellationToken)
    {
        var media = post.Media![0];

        var payload = new Dictionary<string, object?>
        {
            ["file_url"] = media.Path,
            ["description"] = post.Message,
            ["published"] = true
        };

        var url = $"{GraphApiBaseUrl}/{pageId}/videos?access_token={accessToken}&fields=id,permalink_url";
        var request = CreateRequest(url, HttpMethod.Post, payload);

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync(async response =>
            {
                var videoResponse = await DeserializeAsync<FacebookVideoResponse>(response, cancellationToken);
                return new[]
                {
                    new PostResponse
                    {
                        Id = post.Id,
                        PostId = videoResponse.Id,
                        ReleaseUrl = $"https://www.facebook.com/reel/{videoResponse.Id}",
                        Status = "success"
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
        var replyToId = lastCommentId ?? postId;

        var payload = new Dictionary<string, object?>
        {
            ["message"] = commentPost.Message
        };

        if (commentPost.Media != null && commentPost.Media.Count > 0)
        {
            payload["attachment_url"] = commentPost.Media[0].Path;
        }

        var url = $"{GraphApiBaseUrl}/{replyToId}/comments?access_token={accessToken}&fields=id,permalink_url";
        var request = CreateRequest(url, HttpMethod.Post, payload);

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync(async response =>
            {
                var commentResponse = await DeserializeAsync<FacebookPostResponse>(response, cancellationToken);
                return (PostResponse[]?)new[]
                {
                    new PostResponse
                    {
                        Id = commentPost.Id,
                        PostId = commentResponse.Id,
                        ReleaseUrl = commentResponse.PermalinkUrl ?? string.Empty,
                        Status = "success"
                    }
                };
            });
    }

    /// <summary>
    /// Retrieves the list of pages managed by the authenticated user.
    /// </summary>
    /// <param name="accessToken">The user access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of Facebook pages managed by the user.</returns>
    public async Task<Result<List<FacebookPage>, AeroError>> GetPagesAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var url = $"{GraphApiBaseUrl}/me/accounts?fields=id,username,name,picture.type(large)&access_token={accessToken}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync(async response =>
            {
                var pagesResponse = await DeserializeAsync<FacebookPagesResponse>(response, cancellationToken);
                return pagesResponse.Data ?? new List<FacebookPage>();
            });
    }

    /// <inheritdoc/>
    public override async Task<Result<FetchPageInformationResult?, AeroError>> FetchPageInformationAsync(
        string accessToken,
        object data,
        CancellationToken cancellationToken = default)
    {
        var pageId = data.GetType().GetProperty("page")?.GetValue(data)?.ToString() ?? string.Empty;
        var url = $"{GraphApiBaseUrl}/{pageId}?fields=username,access_token,name,picture.type(large)&access_token={accessToken}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync(async response =>
            {
                var page = await DeserializeAsync<FacebookPageDetail>(response, cancellationToken);
                return (FetchPageInformationResult?)new FetchPageInformationResult
                {
                    Id = page.Id,
                    Name = page.Name,
                    AccessToken = page.AccessToken,
                    Picture = page.Picture?.Data?.Url ?? string.Empty,
                    Username = page.Username ?? string.Empty
                };
            });
    }

    private async Task<Result<string, AeroError>> UploadPhotoAsync(string pageId, string accessToken, string photoUrl, bool published, CancellationToken cancellationToken)
    {
        var url = $"{GraphApiBaseUrl}/{pageId}/photos?access_token={accessToken}";
        var payload = new
        {
            url = photoUrl,
            published
        };

        var request = CreateRequest(url, HttpMethod.Post, payload);
        return await SendRequestAsync(request, cancellationToken)
            .BindAsync(async response =>
            {
                var photoResponse = await DeserializeAsync<FacebookPhotoResponse>(response, cancellationToken);
                return photoResponse.Id;
            });
    }

    private async Task<Result<string, AeroError>> ExchangeCodeForTokenAsync(string appId, string appSecret, string redirectUri, string code, CancellationToken cancellationToken)
    {
        var url = $"{GraphApiBaseUrl}/oauth/access_token" +
                  $"?client_id={appId}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&client_secret={appSecret}" +
                  $"&code={code}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendRequestAsync(request, cancellationToken)
            .BindAsync(async response =>
            {
                var tokenResponse = await DeserializeAsync<FacebookAccessTokenResponse>(response, cancellationToken);
                return tokenResponse.AccessToken;
            });
    }

    private async Task<Result<string, AeroError>> ExchangeForLongLivedTokenAsync(string appId, string appSecret, string shortLivedToken, CancellationToken cancellationToken)
    {
        var url = $"{GraphApiBaseUrl}/oauth/access_token" +
                  $"?grant_type=fb_exchange_token" +
                  $"&client_id={appId}" +
                  $"&client_secret={appSecret}" +
                  $"&fb_exchange_token={shortLivedToken}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendRequestAsync(request, cancellationToken)
            .BindAsync(async response =>
            {
                var tokenResponse = await DeserializeAsync<FacebookAccessTokenResponse>(response, cancellationToken);
                return tokenResponse.AccessToken;
            });
    }

    private async Task<Result<string[], AeroError>> GetPermissionsAsync(string accessToken, CancellationToken cancellationToken)
    {
        var url = $"{GraphApiBaseUrl}/me/permissions?access_token={accessToken}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync(async response =>
            {
                var permissionsResponse = await DeserializeAsync<FacebookPermissionsResponse>(response, cancellationToken);
                return permissionsResponse.Data
                    .Where(p => p.Status == "granted")
                    .Select(p => p.Permission)
                    .ToArray();
            });
    }

    private async Task<Result<FacebookUserInfo, AeroError>> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var url = $"{GraphApiBaseUrl}/me?fields=id,name,picture&access_token={accessToken}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        return await SendRequestAsync(request, cancellationToken)
            .BindAsync(async response => await DeserializeAsync<FacebookUserInfo>(response, cancellationToken));
    }

    private static T? GetSettingValue<T>(Dictionary<string, object> settings, string key)
    {
        if (!settings.TryGetValue(key, out var value))
            return default;

        if (value is T typedValue)
            return typedValue;

        var json = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<T>(json);
    }

    //#region DTOs

    private class FacebookAccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }

    private class FacebookPermissionsResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookPermission> Data { get; set; } = new();
    }

    private class FacebookPermission
    {
        [JsonPropertyName("permission")]
        public string Permission { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    private class FacebookUserInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("picture")]
        public FacebookPicture? Picture { get; set; }
    }

    private class FacebookPicture
    {
        [JsonPropertyName("data")]
        public FacebookPictureData? Data { get; set; }
    }

    private class FacebookPictureData
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    private class FacebookPostResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("permalink_url")]
        public string? PermalinkUrl { get; set; }
    }

    private class FacebookVideoResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class FacebookPhotoResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class FacebookPagesResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookPage>? Data { get; set; }
    }

    /// <summary>
    /// Represents a Facebook page.
    /// </summary>
    public class FacebookPage
    {
        /// <summary>
        /// Gets or sets the page ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the page name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the page username.
        /// </summary>
        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }

    private class FacebookPageDetail : FacebookPage
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("picture")]
        public FacebookPicture? Picture { get; set; }
    }

    //#endregion
}
