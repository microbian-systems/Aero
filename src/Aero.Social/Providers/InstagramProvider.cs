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
/// Represents a class for InstagramProvider.
/// </summary>
public class InstagramProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<InstagramProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "instagram";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "Instagram (Facebook Business)";
        /// <summary>
    /// Gets or sets the Is Between Steps.
    /// </summary>
public override bool IsBetweenSteps => true;
        /// <summary>
    /// Gets or sets the Tooltip.
    /// </summary>
public override string? Tooltip => "Instagram must be business and connected to a Facebook page";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => new[]
    {
        "instagram_basic",
        "pages_show_list",
        "pages_read_engagement",
        "business_management",
        "instagram_content_publish",
        "instagram_manage_comments",
        "instagram_manage_insights"
    };
        /// <summary>
    /// Gets or sets the Max Concurrent Jobs.
    /// </summary>
public override int MaxConcurrentJobs => 200;

        /// <summary>
    /// MaxLength method.
    /// </summary>
public override int MaxLength(object? additionalSettings = null) => 2200;

        /// <summary>
    /// HandleErrors method.
    /// </summary>
public new ErrorHandlingResult? HandleErrors(string responseBody)
    {
        if (responseBody.Contains("An unknown error occurred"))
            return new ErrorHandlingResult(ErrorHandlingType.Retry, "An unknown error occurred, please try again later");

        if (responseBody.Contains("REVOKED_ACCESS_TOKEN"))
            return new ErrorHandlingResult(ErrorHandlingType.RefreshToken, "Something is wrong with your connected user, please re-authenticate");

        if (responseBody.ToLower().Contains("the user is not an instagram business"))
            return new ErrorHandlingResult(ErrorHandlingType.RefreshToken, "Your Instagram account is not a business account, please convert it to a business account");

        if (responseBody.ToLower().Contains("session has been invalidated"))
            return new ErrorHandlingResult(ErrorHandlingType.RefreshToken, "Please re-authenticate your Instagram account");

        if (responseBody.Contains("2207050"))
            return new ErrorHandlingResult(ErrorHandlingType.RefreshToken, "Instagram user is restricted");

        if (responseBody.Contains("2207003"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Timeout downloading media, please try again");

        if (responseBody.Contains("2207020"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Media expired, please upload again");

        if (responseBody.Contains("2207010"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Caption is too long");

        if (responseBody.Contains("2207004"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Image is too large");

        if (responseBody.Contains("2207009") || responseBody.Contains("36003"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Aspect ratio not supported, must be between 4:5 to 1.91:1");

        if (responseBody.Contains("2207001"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Instagram detected that your post is spam, please try again with different content");

        if (responseBody.Contains("Page request limit reached"))
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Page posting for today is limited, please try again tomorrow");

        return null;
    }

        /// <summary>
    /// GenerateAuthUrlAsync method.
    /// </summary>
public override async Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(6);
        var appId = GetAppId();
        var frontendUrl = GetFrontendUrl();

        var url = $"https://www.facebook.com/v20.0/dialog/oauth" +
                  $"?client_id={appId}" +
                  $"&redirect_uri={Uri.EscapeDataString($"{frontendUrl}/integrations/social/instagram")}" +
                  $"&state={state}" +
                  $"&scope={Uri.EscapeDataString(string.Join(",", Scopes))}";

        return new GenerateAuthUrlResponse
        {
            Url = url,
            CodeVerifier = MakeId(10),
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
        var appId = GetAppId();
        var appSecret = GetAppSecret();
        var frontendUrl = GetFrontendUrl();
        var redirectUri = $"{frontendUrl}/integrations/social/instagram";

        var shortLivedToken = await ExchangeCodeForTokenAsync(appId, appSecret, redirectUri, parameters.Code, cancellationToken);
        var longLivedToken = await ExchangeForLongLivedTokenAsync(appId, appSecret, shortLivedToken, cancellationToken);

        var permissions = await GetPermissionsAsync(longLivedToken, cancellationToken);
        var scopeCheck = CheckScopes(Scopes, permissions);
        if (scopeCheck.IsFailure) return ((Result<NoneType, AeroError>.Failure)scopeCheck).Error;

        var userInfo = await GetUserInfoAsync(longLivedToken, cancellationToken);

        return new AuthTokenDetails
        {
            Id = userInfo.Id,
            Name = userInfo.Name,
            AccessToken = longLivedToken,
            RefreshToken = longLivedToken,
            ExpiresIn = (int)TimeSpan.FromDays(59).TotalSeconds,
            Picture = userInfo.Picture?.Data?.Url ?? string.Empty,
            Username = string.Empty
        };
    }

        /// <summary>
    /// RefreshTokenAsync method.
    /// </summary>
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
        var isStory = GetSettingValue<string>(settings, "post_type") == "story";

        var mediaIds = new List<string>();

        foreach (var media in firstPost.Media ?? new List<MediaContent>())
        {
            var isVideo = media.Path.Contains(".mp4", StringComparison.OrdinalIgnoreCase);
            var isCarousel = (firstPost.Media?.Count ?? 0) > 1;
            var caption = !isCarousel ? $"&caption={Uri.EscapeDataString(firstPost.Message)}" : "";
            var carouselParam = isCarousel ? "&is_carousel_item=true" : "";

            string mediaType;
            if (isVideo)
            {
                if (!isCarousel && !isStory)
                    mediaType = $"video_url={media.Path}&media_type=REELS&thumb_offset={media.ThumbnailTimestamp ?? 0}";
                else if (isStory)
                    mediaType = $"video_url={media.Path}&media_type=STORIES";
                else
                    mediaType = $"video_url={media.Path}&media_type=VIDEO&thumb_offset={media.ThumbnailTimestamp ?? 0}";
            }
            else
            {
                mediaType = isStory
                    ? $"image_url={media.Path}&media_type=STORIES"
                    : $"image_url={media.Path}";
            }

            var url = $"https://graph.facebook.com/v20.0/{id}/media?{mediaType}{carouselParam}{caption}&access_token={accessToken}";

            var response = await client.PostAsync(url, null, cancellationToken);
            response.EnsureSuccessStatusCode();

            var mediaResponse = await DeserializeAsync<InstagramMediaResponse>(response);
            var photoId = mediaResponse.Id;

            var status = await WaitForMediaProcessingAsync(id, photoId, accessToken, cancellationToken);
            mediaIds.Add(photoId);
        }

        if (mediaIds.Count == 1)
        {
            return await PublishSingleMediaAsync(id, accessToken, firstPost, mediaIds[0], cancellationToken);
        }

        return await PublishCarouselAsync(id, accessToken, firstPost, mediaIds, cancellationToken);
    }

    private async Task<string> WaitForMediaProcessingAsync(string igId, string mediaId, string accessToken, CancellationToken cancellationToken)
    {
        var status = "IN_PROGRESS";

        while (status == "IN_PROGRESS")
        {
            await Task.Delay(5000, cancellationToken);

            var url = $"https://graph.facebook.com/v20.0/{mediaId}?access_token={accessToken}&fields=status_code";
            var response = await client.GetAsync(url, cancellationToken);
            var statusResponse = await DeserializeAsync<InstagramStatusResponse>(response);
            status = statusResponse.StatusCode ?? "ERROR";
        }

        return status;
    }

    private async Task<PostResponse[]> PublishSingleMediaAsync(
        string igId,
        string accessToken,
        PostDetails post,
        string mediaId,
        CancellationToken cancellationToken)
    {
        var url = $"https://graph.facebook.com/v20.0/{igId}/media_publish?creation_id={mediaId}&access_token={accessToken}&field=id";

        var response = await client.PostAsync(url, null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var publishResponse = await DeserializeAsync<InstagramPublishResponse>(response);
        var permalink = await GetMediaPermalinkAsync(igId, publishResponse.Id, accessToken, cancellationToken);

        return new[]
        {
            new PostResponse
            {
                Id = post.Id,
                PostId = publishResponse.Id,
                ReleaseUrl = permalink,
                Status = "success"
            }
        };
    }

    private async Task<PostResponse[]> PublishCarouselAsync(
        string igId,
        string accessToken,
        PostDetails post,
        List<string> mediaIds,
        CancellationToken cancellationToken)
    {
        var children = Uri.EscapeDataString(string.Join(",", mediaIds));
        var caption = Uri.EscapeDataString(post.Message);

        var url = $"https://graph.facebook.com/v20.0/{igId}/media?caption={caption}&media_type=CAROUSEL&children={children}&access_token={accessToken}";

        var response = await client.PostAsync(url, null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var containerResponse = await DeserializeAsync<InstagramMediaResponse>(response);

        await WaitForMediaProcessingAsync(igId, containerResponse.Id, accessToken, cancellationToken);

        var publishUrl = $"https://graph.facebook.com/v20.0/{igId}/media_publish?creation_id={containerResponse.Id}&access_token={accessToken}&field=id";
        var publishResponse = await client.PostAsync(publishUrl, null, cancellationToken);
        publishResponse.EnsureSuccessStatusCode();

        var publishResult = await DeserializeAsync<InstagramPublishResponse>(publishResponse);
        var permalink = await GetMediaPermalinkAsync(igId, publishResult.Id, accessToken, cancellationToken);

        return new[]
        {
            new PostResponse
            {
                Id = post.Id,
                PostId = publishResult.Id,
                ReleaseUrl = permalink,
                Status = "success"
            }
        };
    }

    private async Task<string> GetMediaPermalinkAsync(string igId, string mediaId, string accessToken, CancellationToken cancellationToken)
    {
        var url = $"https://graph.facebook.com/v20.0/{mediaId}?fields=permalink&access_token={accessToken}";
        var response = await client.GetAsync(url, cancellationToken);
        var permalinkResponse = await DeserializeAsync<InstagramPermalinkResponse>(response);
        return permalinkResponse.Permalink ?? $"https://www.instagram.com/p/{mediaId}";
    }

        /// <summary>
    /// CommentAsync method.
    /// </summary>
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
        var message = Uri.EscapeDataString(commentPost.Message);

        var url = $"https://graph.facebook.com/v20.0/{postId}/comments?message={message}&access_token={accessToken}";

        var response = await client.PostAsync(url, null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var commentResponse = await DeserializeAsync<InstagramCommentResponse>(response);
        var permalink = await GetMediaPermalinkAsync(id, postId, accessToken, cancellationToken);

        return new[]
        {
            new PostResponse
            {
                Id = commentPost.Id,
                PostId = commentResponse.Id,
                ReleaseUrl = permalink,
                Status = "success"
            }
        };
    }

        /// <summary>
    /// GetPagesAsync method.
    /// </summary>
public async Task<List<InstagramPage>> GetPagesAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var url = $"https://graph.facebook.com/v20.0/me/accounts?fields=id,instagram_business_account,username,name,picture.type(large)&access_token={accessToken}&limit=500";

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var pagesResponse = await DeserializeAsync<FacebookPagesDataResponse>(response);

        var connectedAccounts = new List<InstagramPage>();
        foreach (var page in pagesResponse.Data ?? new List<FacebookPageData>())
        {
            if (page.InstagramBusinessAccount != null)
            {
                var igUrl = $"https://graph.facebook.com/v20.0/{page.InstagramBusinessAccount.Id}?fields=name,profile_picture_url,username&access_token={accessToken}";
                var igResponse = await client.GetAsync(igUrl, cancellationToken);
                var igData = await DeserializeAsync<InstagramBusinessData>(igResponse);

                connectedAccounts.Add(new InstagramPage
                {
                    PageId = page.Id,
                    Id = page.InstagramBusinessAccount.Id,
                    Name = igData.Name ?? page.Name,
                    Username = igData.Username ?? string.Empty,
                    Picture = igData.ProfilePictureUrl ?? string.Empty
                });
            }
        }

        return connectedAccounts;
    }

    private async Task<string> ExchangeCodeForTokenAsync(string appId, string appSecret, string redirectUri, string code, CancellationToken cancellationToken)
    {
        var url = $"https://graph.facebook.com/v20.0/oauth/access_token" +
                  $"?client_id={appId}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&client_secret={appSecret}" +
                  $"&code={code}";

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await DeserializeAsync<FacebookAccessTokenResponse>(response);
        return tokenResponse.AccessToken;
    }

    private async Task<string> ExchangeForLongLivedTokenAsync(string appId, string appSecret, string shortLivedToken, CancellationToken cancellationToken)
    {
        var url = $"https://graph.facebook.com/v20.0/oauth/access_token" +
                  $"?grant_type=fb_exchange_token" +
                  $"&client_id={appId}" +
                  $"&client_secret={appSecret}" +
                  $"&fb_exchange_token={shortLivedToken}";

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await DeserializeAsync<FacebookAccessTokenResponse>(response);
        return tokenResponse.AccessToken;
    }

    private async Task<string[]> GetPermissionsAsync(string accessToken, CancellationToken cancellationToken)
    {
        var url = $"https://graph.facebook.com/v20.0/me/permissions?access_token={accessToken}";

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var permissionsResponse = await DeserializeAsync<FacebookPermissionsResponse>(response);
        return permissionsResponse.Data
            .Where(p => p.Status == "granted")
            .Select(p => p.Permission)
            .ToArray();
    }

    private async Task<FacebookUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var url = $"https://graph.facebook.com/v20.0/me?fields=id,name,picture&access_token={accessToken}";

        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await DeserializeAsync<FacebookUserInfo>(response);
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

    private string GetAppId() => configuration["FACEBOOK_APP_ID"] ?? throw new InvalidOperationException("FACEBOOK_APP_ID not configured");
    private string GetAppSecret() => configuration["FACEBOOK_APP_SECRET"] ?? throw new InvalidOperationException("FACEBOOK_APP_SECRET not configured");
    private string GetFrontendUrl() => configuration["FRONTEND_URL"] ?? throw new InvalidOperationException("FRONTEND_URL not configured");

    //#region DTOs

    private class InstagramMediaResponse
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class InstagramStatusResponse
    {
                /// <summary>
        /// Gets or sets the Status Code.
        /// </summary>
[JsonPropertyName("status_code")]
        public string? StatusCode { get; set; }
    }

    private class InstagramPublishResponse
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class InstagramPermalinkResponse
    {
                /// <summary>
        /// Gets or sets the Permalink.
        /// </summary>
[JsonPropertyName("permalink")]
        public string? Permalink { get; set; }
    }

    private class InstagramCommentResponse
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class FacebookPagesDataResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public List<FacebookPageData>? Data { get; set; }
    }

    private class FacebookPageData
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
[JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Instagram Business Account.
        /// </summary>
[JsonPropertyName("instagram_business_account")]
        public InstagramBusinessAccountRef? InstagramBusinessAccount { get; set; }
    }

    private class InstagramBusinessAccountRef
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class InstagramBusinessData
    {
                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
[JsonPropertyName("name")]
        public string? Name { get; set; }

                /// <summary>
        /// Gets or sets the Username.
        /// </summary>
[JsonPropertyName("username")]
        public string? Username { get; set; }

                /// <summary>
        /// Gets or sets the Profile Picture Url.
        /// </summary>
[JsonPropertyName("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }
    }

        /// <summary>
    /// Represents a class for InstagramPage.
    /// </summary>
public class InstagramPage
    {
                /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
public string PageId { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
public string Id { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
public string Name { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Username.
        /// </summary>
public string Username { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Picture.
        /// </summary>
public string Picture { get; set; } = string.Empty;
    }

    private class FacebookPermissionsResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public List<FacebookPermission> Data { get; set; } = new();
    }

    private class FacebookPermission
    {
                /// <summary>
        /// Gets or sets the Permission.
        /// </summary>
[JsonPropertyName("permission")]
        public string Permission { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Status.
        /// </summary>
[JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    private class FacebookUserInfo
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
[JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Picture.
        /// </summary>
[JsonPropertyName("picture")]
        public FacebookPicture? Picture { get; set; }
    }

    private class FacebookPicture
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public FacebookPictureData? Data { get; set; }
    }

    private class FacebookPictureData
    {
                /// <summary>
        /// Gets or sets the Url.
        /// </summary>
[JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    private class FacebookAccessTokenResponse
    {
                /// <summary>
        /// Gets or sets the Access Token.
        /// </summary>
[JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Expires In.
        /// </summary>
[JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }

    //#endregion
}
