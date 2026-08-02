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
/// Represents a class for RedditProvider.
/// </summary>
public class RedditProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<RedditProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "reddit";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "Reddit";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => new[] { "read", "identity", "submit", "flair" };
        /// <summary>
    /// Gets or sets the Max Concurrent Jobs.
    /// </summary>
public override int MaxConcurrentJobs => 1;

        /// <summary>
    /// MaxLength method.
    /// </summary>
public override int MaxLength(object? additionalSettings = null) => 10000;

        /// <summary>
    /// GenerateAuthUrlAsync method.
    /// </summary>
public override async Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(6);
        var codeVerifier = MakeId(30);
        var clientId = GetClientId();
        var frontendUrl = GetFrontendUrl();

        var url = $"https://www.reddit.com/api/v1/authorize" +
                  $"?client_id={clientId}" +
                  $"&response_type=code" +
                  $"&state={state}" +
                  $"&redirect_uri={Uri.EscapeDataString($"{frontendUrl}/integrations/social/reddit")}" +
                  $"&duration=permanent" +
                  $"&scope={Uri.EscapeDataString(string.Join(" ", Scopes))}";

        return new GenerateAuthUrlResponse
        {
            Url = url,
            CodeVerifier = codeVerifier,
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
        var clientId = GetClientId();
        var clientSecret = GetClientSecret();
        var frontendUrl = GetFrontendUrl();
        var redirectUri = $"{frontendUrl}/integrations/social/reddit";

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = parameters.Code,
            ["redirect_uri"] = redirectUri
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token")
        {
            Content = content
        };
        request.Headers.Add("Authorization", $"Basic {credentials}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await DeserializeAsync<RedditTokenResponse>(response);
        var scopeCheck = CheckScopes(Scopes, tokenResponse.Scope);
        if (scopeCheck.IsFailure) return ((Result<NoneType, AeroError>.Failure)scopeCheck).Error;

        var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken);

        return new AuthTokenDetails
        {
            Id = userInfo.Id,
            Name = userInfo.Name,
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn,
            Picture = GetCleanIconUrl(userInfo.IconImg),
            Username = userInfo.Name
        };
    }

        /// <summary>
    /// RefreshTokenAsync method.
    /// </summary>
public override async Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var clientId = GetClientId();
        var clientSecret = GetClientSecret();
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token")
        {
            Content = content
        };
        request.Headers.Add("Authorization", $"Basic {credentials}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await DeserializeAsync<RedditTokenResponse>(response);
        var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken);

        return new AuthTokenDetails
        {
            Id = userInfo.Id,
            Name = userInfo.Name,
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn,
            Picture = GetCleanIconUrl(userInfo.IconImg),
            Username = userInfo.Name
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

        var subreddits = GetSettingValue<List<Dictionary<string, object>>>(settings, "subreddit") ?? new List<Dictionary<string, object>>();

        var results = new List<PostResponse>();

        foreach (var subredditConfig in subreddits)
        {
            var subredditSettings = GetSettingValue<Dictionary<string, object>>(subredditConfig, "value") ?? new Dictionary<string, object>();
            var subreddit = GetSettingValue<string>(subredditSettings, "subreddit") ?? string.Empty;
            var title = GetSettingValue<string>(subredditSettings, "title") ?? string.Empty;
            var postType = GetSettingValue<string>(subredditSettings, "type") ?? "self";
            var flairId = GetSettingValue<string>(subredditSettings, "flair_id");
            var url = GetSettingValue<string>(subredditSettings, "url");

            var postData = new Dictionary<string, string>
            {
                ["api_type"] = "json",
                ["title"] = title,
                ["sr"] = subreddit,
                ["text"] = firstPost.Message
            };

            if (!string.IsNullOrEmpty(flairId))
            {
                postData["flair_id"] = flairId;
            }

            if (postType == "link" && !string.IsNullOrEmpty(url))
            {
                postData["kind"] = "link";
                postData["url"] = url;
            }
            else if (postType == "media" && firstPost.Media != null && firstPost.Media.Count > 0)
            {
                var media = firstPost.Media[0];
                var isVideo = media.Path.Contains(".mp4", StringComparison.OrdinalIgnoreCase);

                var uploadedUrl = await UploadFileToRedditAsync(accessToken, media.Path, cancellationToken);

                postData["kind"] = isVideo ? "video" : "image";
                postData["url"] = uploadedUrl;

                if (isVideo && !string.IsNullOrEmpty(media.Thumbnail))
                {
                    var thumbnailUrl = await UploadFileToRedditAsync(accessToken, media.Thumbnail, cancellationToken);
                    postData["video_poster_url"] = thumbnailUrl;
                }
            }
            else
            {
                postData["kind"] = "self";
            }

            var content = new FormUrlEncodedContent(postData);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.reddit.com/api/submit")
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var submitResponse = await DeserializeAsync<RedditSubmitResponse>(response);

            string postId;
            string postUrl;

            if (submitResponse.Json?.Data?.Id != null)
            {
                postId = submitResponse.Json.Data.Id;
                postUrl = submitResponse.Json.Data.Url;
            }
            else if (!string.IsNullOrEmpty(submitResponse.Json?.Data?.WebsocketUrl))
            {
                (postId, postUrl) = await WaitForWebSocketResponseAsync(submitResponse.Json.Data.WebsocketUrl, cancellationToken);
            }
            else
            {
                return AeroError.CreateError("Failed to submit Reddit post");
            }

            results.Add(new PostResponse
            {
                Id = firstPost.Id,
                PostId = postId,
                ReleaseUrl = postUrl,
                Status = "published"
            });

            if (subreddits.Count > 1)
            {
                await Task.Delay(5000, cancellationToken);
            }
        }

        return results.GroupBy(r => r.Id)
            .Select(g => new PostResponse
            {
                Id = g.Key,
                PostId = string.Join(",", g.Select(r => r.PostId)),
                ReleaseUrl = string.Join(",", g.Select(r => r.ReleaseUrl)),
                Status = "published"
            })
            .ToArray();
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
        var thingId = postId.StartsWith("t3_") ? postId : $"t3_{postId}";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["text"] = commentPost.Message,
            ["thing_id"] = thingId,
            ["api_type"] = "json"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.reddit.com/api/comment")
        {
            Content = content
        };
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var commentResponse = await DeserializeAsync<RedditCommentResponse>(response);

        var commentId = commentResponse.Json?.Data?.Things?.FirstOrDefault()?.Data?.Id ?? string.Empty;
        var permalink = commentResponse.Json?.Data?.Things?.FirstOrDefault()?.Data?.Permalink ?? string.Empty;

        return new[]
        {
            new PostResponse
            {
                Id = commentPost.Id,
                PostId = commentId,
                ReleaseUrl = $"https://www.reddit.com{permalink}",
                Status = "published"
            }
        };
    }

        /// <summary>
    /// SearchSubredditsAsync method.
    /// </summary>
public async Task<List<RedditSubreddit>> SearchSubredditsAsync(string accessToken, string query, CancellationToken cancellationToken = default)
    {
        var url = $"https://oauth.reddit.com/subreddits/search?show=public&q={Uri.EscapeDataString(query)}&sort=activity&show_users=false&limit=10";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var searchResponse = await DeserializeAsync<RedditSubredditSearchResponse>(response);

        return searchResponse.Data?.Children?
            .Where(c => c.Data?.SubredditType == "public" && c.Data?.SubmissionType != "image")
            .Select(c => new RedditSubreddit
            {
                Id = c.Data?.Id ?? string.Empty,
                Title = c.Data?.Title ?? string.Empty,
                Name = c.Data?.Url ?? string.Empty
            })
            .ToList() ?? new List<RedditSubreddit>();
    }

    private async Task<string> UploadFileToRedditAsync(string accessToken, string filePath, CancellationToken cancellationToken)
    {
        var fileName = filePath.Split('/').Last();
        var mimeType = GetMimeType(fileName);

        var formData = new MultipartFormDataContent
        {
            { new StringContent(fileName), "filepath" },
            { new StringContent(mimeType), "mimetype" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.reddit.com/api/media/asset")
        {
            Content = formData
        };
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var assetResponse = await DeserializeAsync<RedditAssetResponse>(response);

        var mediaBytesResult = await ReadOrFetchAsync(filePath, cancellationToken);
        if (mediaBytesResult is Result<byte[], AeroError>.Failure)
        {
            return string.Empty;
        }
        var mediaBytes = ((Result<byte[], AeroError>.Ok)mediaBytesResult).Value;

        var uploadForm = new MultipartFormDataContent();
        foreach (var field in assetResponse.Args.Fields)
        {
            uploadForm.Add(new StringContent(field.Value), field.Name);
        }
        uploadForm.Add(new ByteArrayContent(mediaBytes), "file", fileName);

        var uploadResponse = await client.PostAsync("https:" + assetResponse.Args.Action, uploadForm, cancellationToken);
        var uploadResult = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);

        var locationMatch = System.Text.RegularExpressions.Regex.Match(uploadResult, @"<Location>(.*?)</Location>");
        return locationMatch.Success ? locationMatch.Groups[1].Value : string.Empty;
    }

    private async Task<(string id, string url)> WaitForWebSocketResponseAsync(string websocketUrl, CancellationToken cancellationToken)
    {
        await Task.Delay(2000, cancellationToken);
        return (string.Empty, string.Empty);
    }

    private async Task<RedditUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://oauth.reddit.com/api/v1/me");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await DeserializeAsync<RedditUserInfo>(response);
    }

    private static string GetCleanIconUrl(string? iconUrl)
    {
        if (string.IsNullOrEmpty(iconUrl))
            return string.Empty;

        var questionIndex = iconUrl.IndexOf('?');
        return questionIndex > 0 ? iconUrl.Substring(0, questionIndex) : iconUrl;
    }

    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            _ => "application/octet-stream"
        };
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

    private string GetClientId() => configuration["REDDIT_CLIENT_ID"] ?? string.Empty;
    private string GetClientSecret() => configuration["REDDIT_CLIENT_SECRET"] ?? string.Empty;
    private string GetFrontendUrl() => configuration["FRONTEND_URL"] ?? string.Empty;

    //#region DTOs

    private class RedditTokenResponse
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
        /// Gets or sets the Expires In.
        /// </summary>
[JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

                /// <summary>
        /// Gets or sets the Scope.
        /// </summary>
[JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }

    private class RedditUserInfo
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
        /// Gets or sets the Icon Img.
        /// </summary>
[JsonPropertyName("icon_img")]
        public string? IconImg { get; set; }
    }

    private class RedditSubmitResponse
    {
                /// <summary>
        /// Gets or sets the Json.
        /// </summary>
[JsonPropertyName("json")]
        public RedditSubmitJson? Json { get; set; }
    }

    private class RedditSubmitJson
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public RedditSubmitData? Data { get; set; }
    }

    private class RedditSubmitData
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string? Id { get; set; }

                /// <summary>
        /// Gets or sets the Url.
        /// </summary>
[JsonPropertyName("url")]
        public string? Url { get; set; }

                /// <summary>
        /// Gets or sets the Websocket Url.
        /// </summary>
[JsonPropertyName("websocket_url")]
        public string? WebsocketUrl { get; set; }
    }

    private class RedditCommentResponse
    {
                /// <summary>
        /// Gets or sets the Json.
        /// </summary>
[JsonPropertyName("json")]
        public RedditCommentJson? Json { get; set; }
    }

    private class RedditCommentJson
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public RedditCommentData? Data { get; set; }
    }

    private class RedditCommentData
    {
                /// <summary>
        /// Gets or sets the Things.
        /// </summary>
[JsonPropertyName("things")]
        public List<RedditCommentThing>? Things { get; set; }
    }

    private class RedditCommentThing
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public RedditCommentThingData? Data { get; set; }
    }

    private class RedditCommentThingData
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string? Id { get; set; }

                /// <summary>
        /// Gets or sets the Permalink.
        /// </summary>
[JsonPropertyName("permalink")]
        public string? Permalink { get; set; }
    }

    private class RedditAssetResponse
    {
                /// <summary>
        /// Gets or sets the Args.
        /// </summary>
[JsonPropertyName("args")]
        public RedditAssetArgs Args { get; set; } = new();
    }

    private class RedditAssetArgs
    {
                /// <summary>
        /// Gets or sets the Action.
        /// </summary>
[JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Fields.
        /// </summary>
[JsonPropertyName("fields")]
        public List<RedditAssetField> Fields { get; set; } = new();
    }

    private class RedditAssetField
    {
                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
[JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Value.
        /// </summary>
[JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    private class RedditSubredditSearchResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public RedditSubredditSearchData? Data { get; set; }
    }

    private class RedditSubredditSearchData
    {
                /// <summary>
        /// Gets or sets the Children.
        /// </summary>
[JsonPropertyName("children")]
        public List<RedditSubredditChild>? Children { get; set; }
    }

    private class RedditSubredditChild
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public RedditSubredditChildData? Data { get; set; }
    }

    private class RedditSubredditChildData
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string? Id { get; set; }

                /// <summary>
        /// Gets or sets the Title.
        /// </summary>
[JsonPropertyName("title")]
        public string? Title { get; set; }

                /// <summary>
        /// Gets or sets the Url.
        /// </summary>
[JsonPropertyName("url")]
        public string? Url { get; set; }

                /// <summary>
        /// Gets or sets the Subreddit Type.
        /// </summary>
[JsonPropertyName("subreddit_type")]
        public string? SubredditType { get; set; }

                /// <summary>
        /// Gets or sets the Submission Type.
        /// </summary>
[JsonPropertyName("submission_type")]
        public string? SubmissionType { get; set; }
    }

        /// <summary>
    /// Represents a class for RedditSubreddit.
    /// </summary>
public class RedditSubreddit
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
public string Id { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Title.
        /// </summary>
public string Title { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
public string Name { get; set; } = string.Empty;
    }

    //#endregion
}
