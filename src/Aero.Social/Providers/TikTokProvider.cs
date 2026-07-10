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
/// Represents a class for TikTokProvider.
/// </summary>
public class TikTokProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<TikTokProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "tiktok";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "TikTok";
        /// <summary>
    /// Gets or sets the Convert To Jpeg.
    /// </summary>
public override bool ConvertToJpeg => true;
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => new[]
    {
        "video.list",
        "user.info.basic",
        "video.publish",
        "video.upload",
        "user.info.profile",
        "user.info.stats"
    };
        /// <summary>
    /// Gets or sets the Max Concurrent Jobs.
    /// </summary>
public override int MaxConcurrentJobs => 1;

        /// <summary>
    /// MaxLength method.
    /// </summary>
public override int MaxLength(object? additionalSettings = null) => 2000;

        /// <summary>
    /// HandleErrors method.
    /// </summary>
protected override ErrorHandlingResult? HandleErrors(string responseBody)
    {
        if (responseBody.Contains("access_token_invalid"))
            return new ErrorHandlingResult(ErrorHandlingType.RefreshToken, "Access token invalid, please re-authenticate your TikTok account");

        if (responseBody.Contains("scope_not_authorized"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Missing required permissions, please re-authenticate with all scopes");

        if (responseBody.Contains("rate_limit_exceeded"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "TikTok API rate limit exceeded, please try again later");

        if (responseBody.Contains("file_format_check_failed"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "File format is invalid, please check video specifications");

        if (responseBody.Contains("duration_check_failed"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Video duration is invalid, please check video specifications");

        if (responseBody.Contains("video_pull_failed"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Failed to pull video from URL, please check the URL");

        if (responseBody.Contains("photo_pull_failed"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Failed to pull photo from URL, please check the URL");

        if (responseBody.Contains("spam_risk_user_banned_from_posting"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Account banned from posting, please check TikTok account status");

        if (responseBody.Contains("spam_risk_text"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "TikTok detected potential spam in the post text");

        if (responseBody.Contains("spam_risk_too_many_posts"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Daily post limit reached, please try again tomorrow");

        if (responseBody.Contains("picture_size_check_failed"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Picture/Video size is invalid, must be at least 720p");

        if (responseBody.Contains("internal"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "There is a problem with TikTok servers, please try again later");

        return null;
    }

        /// <summary>
    /// GenerateAuthUrlAsync method.
    /// </summary>
public override async Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = Guid.NewGuid().ToString("N").Substring(0, 8);
        var clientKey = GetClientId();
        var frontendUrl = GetFrontendUrl();
        var redirectUri = $"{frontendUrl}/integrations/social/tiktok";

        var url = $"https://www.tiktok.com/v2/auth/authorize/" +
                  $"?client_key={clientKey}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&state={state}" +
                  $"&response_type=code" +
                  $"&scope={Uri.EscapeDataString(string.Join(",", Scopes))}";

        return new GenerateAuthUrlResponse
        {
            Url = url,
            CodeVerifier = state,
            State = state
        };
    }

        /// <summary>
    /// AuthenticateAsync method.
    /// </summary>
public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var clientKey = GetClientId();
        var clientSecret = GetClientSecret();
        var frontendUrl = GetFrontendUrl();
        var redirectUri = $"{frontendUrl}/integrations/social/tiktok";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_key"] = clientKey,
            ["client_secret"] = clientSecret,
            ["code"] = parameters.Code,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = parameters.CodeVerifier,
            ["redirect_uri"] = redirectUri
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://open.tiktokapis.com/v2/oauth/token/")
        {
            Content = content
        };

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await DeserializeAsync<TikTokTokenResponse>(response);
        var scopeCheck = CheckScopes(Scopes, tokenResponse.Scope);
        if (scopeCheck.IsFailure) return ((Result<NoneType, AeroError>.Failure)scopeCheck).Error;

        var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken);

        return new AuthTokenDetails
        {
            Id = userInfo.OpenId.Replace("-", ""),
            Name = userInfo.DisplayName,
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = (int)TimeSpan.FromHours(23).TotalSeconds,
            Picture = userInfo.AvatarUrl ?? string.Empty,
            Username = userInfo.Username ?? string.Empty
        };
    }

        /// <summary>
    /// RefreshTokenAsync method.
    /// </summary>
public override async Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var clientKey = GetClientId();
        var clientSecret = GetClientSecret();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_key"] = clientKey,
            ["client_secret"] = clientSecret,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://open.tiktokapis.com/v2/oauth/token/")
        {
            Content = content
        };

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await DeserializeAsync<TikTokTokenResponse>(response);
        var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken);

        return new AuthTokenDetails
        {
            Id = userInfo.OpenId.Replace("-", ""),
            Name = userInfo.DisplayName,
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = (int)TimeSpan.FromHours(23).TotalSeconds,
            Picture = userInfo.AvatarUrl ?? string.Empty,
            Username = userInfo.Username ?? string.Empty
        };
    }

        /// <summary>
    /// PostAsync method.
    /// </summary>
public override async Task<Result<PostResponse[], AeroError>> PostAsync(
        string id,
        string accessToken,
        List<PostDetails> posts,
        Integration integration,
        CancellationToken cancellationToken = default)
    {
        var firstPost = posts.First();
        var settings = firstPost.Settings ?? new Dictionary<string, object>();

        var isPhoto = firstPost.Media == null || !firstPost.Media[0].Path.Contains(".mp4", StringComparison.OrdinalIgnoreCase);
        var postingMethod = GetSettingValue<string>(settings, "content_posting_method") ?? "DIRECT_POST";

        var endpoint = GetPostingEndpoint(postingMethod, isPhoto);

        var payload = BuildPostPayload(firstPost, settings, isPhoto, postingMethod);

        var request = CreateRequest($"https://open.tiktokapis.com/v2/post/publish{endpoint}", HttpMethod.Post, payload);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("Content-Type", "application/json; charset=UTF-8");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var publishResponse = await DeserializeAsync<TikTokPublishResponse>(response);

        var (url, postId) = await WaitForPublishCompleteAsync(integration.Username ?? id, publishResponse.Data.PublishId, accessToken, cancellationToken);

        return new[]
        {
            new PostResponse
            {
                Id = firstPost.Id,
                ReleaseUrl = url,
                PostId = postId,
                Status = "success"
            }
        };
    }

    private static string GetPostingEndpoint(string method, bool isPhoto)
    {
        return method switch
        {
            "UPLOAD" => isPhoto ? "/content/init/" : "/inbox/video/init/",
            _ => isPhoto ? "/content/init/" : "/video/init/"
        };
    }

    private static object BuildPostPayload(PostDetails post, Dictionary<string, object> settings, bool isPhoto, string postingMethod)
    {
        var privacyLevel = GetSettingValue<string>(settings, "privacy_level") ?? "PUBLIC_TO_EVERYONE";
        var duet = GetSettingValue<bool?>(settings, "duet") ?? false;
        var comment = GetSettingValue<bool?>(settings, "comment") ?? false;
        var stitch = GetSettingValue<bool?>(settings, "stitch") ?? false;
        var videoMadeWithAi = GetSettingValue<bool?>(settings, "video_made_with_ai") ?? false;
        var brandContentToggle = GetSettingValue<bool?>(settings, "brand_content_toggle") ?? false;
        var brandOrganicToggle = GetSettingValue<bool?>(settings, "brand_organic_toggle") ?? false;
        var title = GetSettingValue<string>(settings, "title");
        var autoAddMusic = GetSettingValue<string>(settings, "autoAddMusic") == "yes";

        var payload = new Dictionary<string, object>();

        if (postingMethod == "DIRECT_POST")
        {
            var postInfo = new Dictionary<string, object>
            {
                ["privacy_level"] = privacyLevel,
                ["disable_duet"] = !duet,
                ["disable_comment"] = !comment,
                ["disable_stitch"] = !stitch,
                ["is_aigc"] = videoMadeWithAi,
                ["brand_content_toggle"] = brandContentToggle,
                ["brand_organic_toggle"] = brandOrganicToggle
            };

            if (!string.IsNullOrEmpty(title) && isPhoto)
                postInfo["title"] = title;
            else if (!string.IsNullOrEmpty(post.Message) && !isPhoto)
                postInfo["title"] = post.Message;

            if (isPhoto)
                postInfo["description"] = post.Message;

            if (isPhoto)
                postInfo["auto_add_music"] = autoAddMusic;

            payload["post_info"] = postInfo;
        }

        if (isPhoto && post.Media != null)
        {
            payload["source_info"] = new
            {
                source = "PULL_FROM_URL",
                photo_cover_index = 0,
                photo_images = post.Media.Select(m => m.Path).ToArray()
            };
            payload["post_mode"] = postingMethod == "DIRECT_POST" ? "DIRECT_POST" : "MEDIA_UPLOAD";
            payload["media_type"] = "PHOTO";
        }
        else if (post.Media != null && post.Media.Count > 0)
        {
            var media = post.Media[0];
            var sourceInfo = new Dictionary<string, object>
            {
                ["source"] = "PULL_FROM_URL",
                ["video_url"] = media.Path
            };

            if (media.ThumbnailTimestamp.HasValue)
                sourceInfo["video_cover_timestamp_ms"] = media.ThumbnailTimestamp.Value;

            payload["source_info"] = sourceInfo;
        }

        return payload;
    }

    private async Task<(string url, string id)> WaitForPublishCompleteAsync(string username, string publishId, string accessToken, CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(10000, cancellationToken);

            var payload = new { publish_id = publishId };
            var request = CreateRequest("https://open.tiktokapis.com/v2/post/publish/status/fetch/", HttpMethod.Post, payload);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("Content-Type", "application/json; charset=UTF-8");

            var response = await client.SendAsync(request, cancellationToken);
            var statusResponse = await DeserializeAsync<TikTokStatusResponse>(response);

            var status = statusResponse.Data?.Status;

            if (status == "SEND_TO_USER_INBOX")
            {
                return ("https://www.tiktok.com/tiktokstudio/content?tab=post", Random.Shared.Next(100000, 1000000).ToString());
            }

            if (status == "PUBLISH_COMPLETE")
            {
                var postId = statusResponse.Data?.PublicalyAvailablePostId?.FirstOrDefault();
                var url = postId != null
                    ? $"https://www.tiktok.com/@{username}/video/{postId}"
                    : $"https://www.tiktok.com/@{username}";

                return (url, postId?.ToString() ?? publishId);
            }

            if (status == "FAILED")
            {
                var errorBody = JsonSerializer.Serialize(statusResponse);
                var handleError = HandleErrors(errorBody);
                return ("", "");
            }
        }
    }

    private async Task<TikTokUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            "https://open.tiktokapis.com/v2/user/info/?fields=open_id,avatar_url,display_name,union_id,username");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var userInfoResponse = await DeserializeAsync<TikTokUserInfoResponse>(response);
        return userInfoResponse.Data.User;
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

    private string GetClientId() => configuration["TIKTOK_CLIENT_ID"] ?? string.Empty;
    private string GetClientSecret() => configuration["TIKTOK_CLIENT_SECRET"] ?? string.Empty;
    private string GetFrontendUrl() => configuration["FRONTEND_URL"] ?? string.Empty;

    //#region DTOs

    private class TikTokTokenResponse
    {
                /// <summary>
        /// Gets or sets the Access Token.
        /// </summary>
[JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Refresh Token.
        /// </summary>
[JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Scope.
        /// </summary>
[JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }

    private class TikTokUserInfoResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public TikTokUserData Data { get; set; } = new();
    }

    private class TikTokUserData
    {
                /// <summary>
        /// Gets or sets the User.
        /// </summary>
[JsonPropertyName("user")]
        public TikTokUserInfo User { get; set; } = new();
    }

    private class TikTokUserInfo
    {
                /// <summary>
        /// Gets or sets the Open Id.
        /// </summary>
[JsonPropertyName("open_id")]
        public string OpenId { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Display Name.
        /// </summary>
[JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Avatar Url.
        /// </summary>
[JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

                /// <summary>
        /// Gets or sets the Username.
        /// </summary>
[JsonPropertyName("username")]
        public string? Username { get; set; }
    }

    private class TikTokPublishResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public TikTokPublishData Data { get; set; } = new();
    }

    private class TikTokPublishData
    {
                /// <summary>
        /// Gets or sets the Publish Id.
        /// </summary>
[JsonPropertyName("publish_id")]
        public string PublishId { get; set; } = string.Empty;
    }

    private class TikTokStatusResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public TikTokStatusData? Data { get; set; }
    }

    private class TikTokStatusData
    {
                /// <summary>
        /// Gets or sets the Status.
        /// </summary>
[JsonPropertyName("status")]
        public string? Status { get; set; }

                /// <summary>
        /// Gets or sets the Publicaly Available Post Id.
        /// </summary>
[JsonPropertyName("publicaly_available_post_id")]
        public List<long>? PublicalyAvailablePostId { get; set; }
    }

    //#endregion
}
