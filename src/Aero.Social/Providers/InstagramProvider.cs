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
/// Provides integration with Instagram (via Facebook Business) for posting and analytics.
/// </summary>
public class InstagramProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<InstagramProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    private const string GraphApiBaseUrl = "https://graph.facebook.com/v20.0";

    /// <inheritdoc/>
    public override string Identifier => "instagram";

    /// <inheritdoc/>
    public override string Name => "Instagram (Facebook Business)";

    /// <inheritdoc/>
    public override bool IsBetweenSteps => true;

    /// <inheritdoc/>
    public override string? Tooltip => "Instagram must be business and connected to a Facebook page";

    /// <inheritdoc/>
    public override string[] Scopes =>
    [
        "instagram_basic",
        "pages_show_list",
        "pages_read_engagement",
        "business_management",
        "instagram_content_publish",
        "instagram_manage_comments",
        "instagram_manage_insights"
    ];

    /// <inheritdoc/>
    public override int MaxConcurrentJobs => 200;

    /// <inheritdoc/>
    public override int MaxLength(object? additionalSettings = null) => 2200;

    /// <inheritdoc/>
    protected override ErrorHandlingResult? HandleErrors(string responseBody)
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

    /// <inheritdoc/>
    public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        return GetAppId().Bind(appId =>
            GetFrontendUrl().Map(frontendUrl =>
            {
                var state = MakeId(6);
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
            })).AsTask();
    }

    /// <inheritdoc/>
    public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        return await GetAuthenticationConfig().BindAsync(config =>
        {
            var redirectUri = $"{config.FrontendUrl}/integrations/social/instagram";

            return ExchangeCodeForTokenAsync(config.AppId, config.AppSecret, redirectUri, parameters.Code, cancellationToken)
                .BindAsync(shortToken => ExchangeForLongLivedTokenAsync(config.AppId, config.AppSecret, shortToken, cancellationToken))
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
                                .MapAsync(userInfo => new AuthTokenDetails
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
        });
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

            var url = $"{GraphApiBaseUrl}/{id}/media?{mediaType}{carouselParam}{caption}&access_token={accessToken}";

            var result = await PostAsync(url, (object)null!, cancellationToken);
            if (result is Result<HttpResponseMessage, AeroError>.Failure failure) return failure.Error;

            var response = ((Result<HttpResponseMessage, AeroError>.Ok)result).Value;
            var mediaResponse = await DeserializeAsync<InstagramMediaResponse>(response, cancellationToken);
            var photoId = mediaResponse.Id;

            var statusResult = await WaitForMediaProcessingAsync(accessToken, photoId, cancellationToken);
            if (statusResult is Result<string, AeroError>.Failure statusFailure) return statusFailure.Error;

            mediaIds.Add(photoId);
        }

        if (mediaIds.Count == 1)
        {
            return await PublishSingleMediaAsync(id, accessToken, firstPost, mediaIds[0], cancellationToken);
        }

        return await PublishCarouselAsync(id, accessToken, firstPost, mediaIds, cancellationToken);
    }

    public async Task<Result<string, AeroError>> WaitForMediaProcessingAsync(string accessToken, string id, CancellationToken cancellationToken)
    {
        var url = $"https://graph.facebook.com/v21.0/{id}?fields=status,status_code&access_token={accessToken}";
        for (int i = 0; i < 30; i++)
        {
            var result = await SendRequestAsync<InstagramMediaStatus>(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken);
            if (result.IsFailure) return result.Map(_ => string.Empty);

            var media = result.GetValueOrThrow();

            if (media.StatusCode == "FINISHED")
                return id;

            if (media.StatusCode == "ERROR")
                return AeroError.HttpRequestError(System.Net.HttpStatusCode.BadRequest, $"Media processing failed: {media.Status}");

            await Task.Delay(2000, cancellationToken);
        }

        return AeroError.HttpRequestError(System.Net.HttpStatusCode.RequestTimeout, "Media processing timed out");
    }

    private async Task<Result<PostResponse[], AeroError>> PublishSingleMediaAsync(
        string igId,
        string accessToken,
        PostDetails post,
        string mediaId,
        CancellationToken cancellationToken)
    {
        var url = $"{GraphApiBaseUrl}/{igId}/media_publish?creation_id={mediaId}&access_token={accessToken}&field=id";

        var result = await PostAsync(url, (object)null!, cancellationToken);
        if (result is Result<HttpResponseMessage, AeroError>.Failure failure) return failure.Error;

        var response = ((Result<HttpResponseMessage, AeroError>.Ok)result).Value;
        var publishResponse = await DeserializeAsync<InstagramPublishResponse>(response, cancellationToken);
        var permalinkResult = await GetMediaPermalinkAsync(igId, publishResponse.Id, accessToken, cancellationToken);

        return new[]
        {
            new PostResponse
            {
                Id = post.Id,
                PostId = publishResponse.Id,
                ReleaseUrl = permalinkResult.IsSuccess ? permalinkResult.Value : $"https://www.instagram.com/p/{publishResponse.Id}",
                Status = "success"
            }
        };
    }

    private async Task<Result<PostResponse[], AeroError>> PublishCarouselAsync(
        string igId,
        string accessToken,
        PostDetails post,
        List<string> mediaIds,
        CancellationToken cancellationToken)
    {
        var children = Uri.EscapeDataString(string.Join(",", mediaIds));
        var caption = Uri.EscapeDataString(post.Message);

        var url = $"{GraphApiBaseUrl}/{igId}/media?caption={caption}&media_type=CAROUSEL&children={children}&access_token={accessToken}";

        var result = await PostAsync(url, (object)null!, cancellationToken);
        if (result is Result<HttpResponseMessage, AeroError>.Failure failure) return failure.Error;

        var response = ((Result<HttpResponseMessage, AeroError>.Ok)result).Value;
        var containerResponse = await DeserializeAsync<InstagramMediaResponse>(response, cancellationToken);

        var statusResult = await WaitForMediaProcessingAsync(accessToken, containerResponse.Id, cancellationToken);
        if (statusResult is Result<string, AeroError>.Failure statusFailure) return statusFailure.Error;

        var publishUrl = $"{GraphApiBaseUrl}/{igId}/media_publish?creation_id={containerResponse.Id}&access_token={accessToken}&field=id";
        var publishResult = await PostAsync(publishUrl, (object)null!, cancellationToken);
        if (publishResult is Result<HttpResponseMessage, AeroError>.Failure publishFailure) return publishFailure.Error;

        var publishResponse = ((Result<HttpResponseMessage, AeroError>.Ok)publishResult).Value;
        var publishData = await DeserializeAsync<InstagramPublishResponse>(publishResponse, cancellationToken);
        var permalinkResult = await GetMediaPermalinkAsync(igId, publishData.Id, accessToken, cancellationToken);

        return new[]
        {
            new PostResponse
            {
                Id = post.Id,
                PostId = publishData.Id,
                ReleaseUrl = permalinkResult.IsSuccess ? permalinkResult.Value : $"https://www.instagram.com/p/{publishData.Id}",
                Status = "success"
            }
        };
    }

    private async Task<Result<string, AeroError>> GetMediaPermalinkAsync(string igId, string mediaId, string accessToken, CancellationToken cancellationToken)
    {
        var url = $"{GraphApiBaseUrl}/{mediaId}?fields=permalink&access_token={accessToken}";
        var result = await GetAsync(url, cancellationToken);
        
        return await result.BindAsync(async response => 
        {
            var permalinkResponse = await DeserializeAsync<InstagramPermalinkResponse>(response, cancellationToken);
            return permalinkResponse.Permalink ?? $"https://www.instagram.com/p/{mediaId}";
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
        var message = Uri.EscapeDataString(commentPost.Message);

        var url = $"{GraphApiBaseUrl}/{postId}/comments?message={message}&access_token={accessToken}";

        var result = await PostAsync(url, (object)null!, cancellationToken);
        if (result is Result<HttpResponseMessage, AeroError>.Failure failure) return failure.Error;

        var response = ((Result<HttpResponseMessage, AeroError>.Ok)result).Value;
        var commentResponse = await DeserializeAsync<InstagramCommentResponse>(response, cancellationToken);
        var permalinkResult = await GetMediaPermalinkAsync(id, postId, accessToken, cancellationToken);

        return new[]
        {
            new PostResponse
            {
                Id = commentPost.Id,
                PostId = commentResponse.Id,
                ReleaseUrl = permalinkResult.IsSuccess ? permalinkResult.Value : $"https://www.instagram.com/p/{postId}",
                Status = "success"
            }
        };
    }

    /// <summary>
    /// Retrieves the list of Instagram business accounts connected to the user's Facebook pages.
    /// </summary>
    /// <param name="accessToken">The user access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of connected Instagram accounts.</returns>
    public async Task<Result<List<InstagramPage>, AeroError>> GetPagesAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var url = $"{GraphApiBaseUrl}/me/accounts?fields=id,instagram_business_account,username,name,picture.type(large)&access_token={accessToken}&limit=500";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var result = await SendRequestAsync(request, cancellationToken);

        return await result.BindAsync(async response =>
        {
            var pagesResponse = await DeserializeAsync<FacebookPagesDataResponse>(response, cancellationToken);
            var connectedAccounts = new List<InstagramPage>();

            foreach (var page in pagesResponse.Data ?? new List<FacebookPageData>())
            {
                if (page.InstagramBusinessAccount != null)
                {
                    var igUrl = $"{GraphApiBaseUrl}/{page.InstagramBusinessAccount.Id}?fields=name,profile_picture_url,username&access_token={accessToken}";
                    var igRequest = new HttpRequestMessage(HttpMethod.Get, igUrl);
                    var igResult = await SendRequestAsync(igRequest, cancellationToken);

                    if (igResult is Result<HttpResponseMessage, AeroError>.Failure igFailure)
                        return igFailure.Error;

                    var igResponse = ((Result<HttpResponseMessage, AeroError>.Ok)igResult).Value;
                    var igData = await DeserializeAsync<InstagramBusinessData>(igResponse, cancellationToken);

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
        var result = await SendRequestAsync(request, cancellationToken);

        return await result.BindAsync(async response =>
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
        var result = await SendRequestAsync(request, cancellationToken);

        return await result.BindAsync(async response =>
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
            .MapAsync(async response =>
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

    private class InstagramMediaResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class InstagramStatusResponse
    {
        [JsonPropertyName("status_code")]
        public string? StatusCode { get; set; }
    }

    private class InstagramPublishResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class InstagramMediaStatus
    {
        [JsonPropertyName("status_code")]
        public string? StatusCode { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    private class InstagramPermalinkResponse
    {
        [JsonPropertyName("permalink")]
        public string? Permalink { get; set; }
    }

    private class InstagramCommentResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class FacebookPagesDataResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookPageData>? Data { get; set; }
    }

    private class FacebookPageData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("instagram_business_account")]
        public InstagramBusinessAccountRef? InstagramBusinessAccount { get; set; }
    }

    private class InstagramBusinessAccountRef
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class InstagramBusinessData
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }
    }

    /// <summary>
    /// Represents an Instagram page connected via Facebook.
    /// </summary>
    public class InstagramPage
    {
        /// <summary>
        /// Gets or sets the Facebook page ID.
        /// </summary>
        public string PageId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Instagram business account ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account profile picture URL.
        /// </summary>
        public string Picture { get; set; } = string.Empty;
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

    private class FacebookAccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }

    //#endregion
    private Result<string, AeroError> GetAppId() => configuration["FACEBOOK_APP_ID"] ?? AeroError.CreateError("FACEBOOK_APP_ID not configured");
    private Result<string, AeroError> GetAppSecret() => configuration["FACEBOOK_APP_SECRET"] ?? AeroError.CreateError("FACEBOOK_APP_SECRET not configured");
    private Result<string, AeroError> GetFrontendUrl() => configuration["FRONTEND_URL"] ?? AeroError.CreateError("FRONTEND_URL not configured");

    private Result<(string AppId, string AppSecret, string FrontendUrl), AeroError> GetAuthenticationConfig()
    {
        return GetAppId().Bind(appId =>
            GetAppSecret().Bind(appSecret =>
                GetFrontendUrl().Map(frontendUrl => (appId, appSecret, frontendUrl))));
    }
}
