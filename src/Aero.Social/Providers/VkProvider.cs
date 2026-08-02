using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Aero.Core;
using Aero.Core.Railway;
using Aero.Social.Abstractions;
using Aero.Social.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Providers;

/// <summary>
/// Represents a class for VkProvider.
/// </summary>
public class VkProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<VkProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "vk";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "VK";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => new[]
    {
        "vkid.personal_info",
        "email",
        "wall",
        "status",
        "docs",
        "photos",
        "video"
    };

        /// <summary>
    /// Gets or sets the Max Concurrent Jobs.
    /// </summary>
public override int MaxConcurrentJobs => 2;
        /// <summary>
    /// MaxLength method.
    /// </summary>
public override int MaxLength(object? additionalSettings = null) => 2048;

        /// <summary>
    /// GenerateAuthUrlAsync method.
    /// </summary>
public override async Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(32);
        var codeVerifier = GenerateCodeVerifier();
        var challenge = GenerateCodeChallenge(codeVerifier);

        var clientId = GetClientId();
        var frontendUrl = GetFrontendUrl();
        var redirectUri = frontendUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase)
            ? $"{frontendUrl}/integrations/social/vk"
            : $"https://redirectmeto.com/{frontendUrl}/integrations/social/vk";

        var url = $"https://id.vk.com/authorize" +
                  $"?response_type=code" +
                  $"&client_id={clientId}" +
                  $"&code_challenge_method=S256" +
                  $"&code_challenge={challenge}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&state={state}" +
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
        var codeParts = parameters.Code.Split("&&&&");
        var code = codeParts[0];
        var deviceId = codeParts.Length > 1 ? codeParts[1] : MakeId(32);

        var clientId = GetClientId();
        var frontendUrl = GetFrontendUrl();
        var redirectUri = frontendUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase)
            ? $"{frontendUrl}/integrations/social/vk"
            : $"https://redirectmeto.com/{frontendUrl}/integrations/social/vk";

        var formData = new MultipartFormDataContent
        {
            { new StringContent(clientId), "client_id" },
            { new StringContent("authorization_code"), "grant_type" },
            { new StringContent(parameters.CodeVerifier ?? string.Empty), "code_verifier" },
            { new StringContent(deviceId), "device_id" },
            { new StringContent(code), "code" },
            { new StringContent(redirectUri), "redirect_uri" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://id.vk.com/oauth2/auth")
        {
            Content = formData
        };

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await DeserializeAsync<VkTokenResponse>(response);

        var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken);

        return new AuthTokenDetails
        {
            Id = userInfo.UserId,
            Name = $"{userInfo.FirstName} {userInfo.LastName}",
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = $"{tokenResponse.RefreshToken}&&&&{deviceId}",
            ExpiresIn = tokenResponse.ExpiresIn,
            Picture = userInfo.Avatar ?? string.Empty,
            Username = userInfo.FirstName.ToLowerInvariant()
        };
    }

        /// <summary>
    /// RefreshTokenAsync method.
    /// </summary>
public override async Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var parts = refreshToken.Split("&&&&");
        var oldRefreshToken = parts[0];
        var deviceId = parts.Length > 1 ? parts[1] : MakeId(32);

        var clientId = GetClientId();

        var formData = new MultipartFormDataContent
        {
            { new StringContent("refresh_token"), "grant_type" },
            { new StringContent(oldRefreshToken), "refresh_token" },
            { new StringContent(clientId), "client_id" },
            { new StringContent(deviceId), "device_id" },
            { new StringContent(MakeId(32)), "state" },
            { new StringContent(string.Join(" ", Scopes)), "scope" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://id.vk.com/oauth2/auth")
        {
            Content = formData
        };

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await DeserializeAsync<VkTokenResponse>(response);
        var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken, cancellationToken);

        return new AuthTokenDetails
        {
            Id = userInfo.UserId,
            Name = $"{userInfo.FirstName} {userInfo.LastName}",
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = $"{tokenResponse.RefreshToken}&&&&{deviceId}",
            ExpiresIn = tokenResponse.ExpiresIn,
            Picture = userInfo.Avatar ?? string.Empty,
            Username = userInfo.FirstName.ToLowerInvariant()
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

        var mediaListResult = await UploadMediaAsync(id, accessToken, firstPost, cancellationToken);
        if (mediaListResult is Result<List<VkMedia>, AeroError>.Failure failure) return failure.Error;
        var mediaList = ((Result<List<VkMedia>, AeroError>.Ok)mediaListResult).Value;

        var formData = new MultipartFormDataContent
        {
            { new StringContent(firstPost.Message), "message" }
        };

        if (mediaList.Count > 0)
        {
            var attachments = string.Join(",", mediaList.Select(m => $"{m.Type}{id}_{m.Id}"));
            formData.Add(new StringContent(attachments), "attachments");
        }

        var clientId = GetClientId();
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.vk.com/method/wall.post?v=5.251&access_token={accessToken}&client_id={clientId}")
        {
            Content = formData
        };

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var postResponse = await DeserializeAsync<VkWallPostResponse>(response);

        return new[]
        {
            new PostResponse
            {
                Id = firstPost.Id,
                PostId = postResponse.Response?.PostId.ToString() ?? string.Empty,
                ReleaseUrl = $"https://vk.com/feed?w=wall{id}_{postResponse.Response?.PostId}",
                Status = "completed"
            }
        };
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

        var mediaListResult = await UploadMediaAsync(id, accessToken, commentPost, cancellationToken);
        if (mediaListResult is Result<List<VkMedia>, AeroError>.Failure failure) return failure.Error;
        var mediaList = ((Result<List<VkMedia>, AeroError>.Ok)mediaListResult).Value;

        var formData = new MultipartFormDataContent
        {
            { new StringContent(commentPost.Message), "message" },
            { new StringContent(postId), "post_id" }
        };

        if (mediaList.Count > 0)
        {
            var attachments = string.Join(",", mediaList.Select(m => $"{m.Type}{id}_{m.Id}"));
            formData.Add(new StringContent(attachments), "attachments");
        }

        var clientId = GetClientId();
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.vk.com/method/wall.createComment?v=5.251&access_token={accessToken}&client_id={clientId}")
        {
            Content = formData
        };

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var commentResponse = await DeserializeAsync<VkCommentResponse>(response);

        return new[]
        {
            new PostResponse
            {
                Id = commentPost.Id,
                PostId = commentResponse.Response?.CommentId.ToString() ?? string.Empty,
                ReleaseUrl = $"https://vk.com/feed?w=wall{id}_{postId}",
                Status = "completed"
            }
        };
    }

    private async Task<VkUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var clientId = GetClientId();

        var formData = new MultipartFormDataContent
        {
            { new StringContent(clientId), "client_id" },
            { new StringContent(accessToken), "access_token" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://id.vk.com/oauth2/user_info")
        {
            Content = formData
        };

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var userInfoResponse = await DeserializeAsync<VkUserInfoResponse>(response);
        return userInfoResponse.User;
    }

    private async Task<Result<List<VkMedia>, AeroError>> UploadMediaAsync(string userId, string accessToken, PostDetails post, CancellationToken cancellationToken)
    {
        var mediaList = new List<VkMedia>();

        if (post.Media == null || post.Media.Count == 0)
        {
            return mediaList;
        }

        foreach (var media in post.Media)
        {
            var isVideo = media.Path.Contains(".mp4", StringComparison.OrdinalIgnoreCase);

            if (isVideo)
            {
                var uploadServerRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.vk.com/method/video.save?access_token={accessToken}&v=5.251");
                var uploadServerResponse = await client.SendAsync(uploadServerRequest, cancellationToken);
                var uploadServerData = await DeserializeAsync<VkVideoUploadServerResponse>(uploadServerResponse);

                var mediaBytesResult = await ReadOrFetchAsync(media.Path, cancellationToken);
                if (mediaBytesResult is Result<byte[], AeroError>.Failure failure) return failure.Error;
                var mediaBytes = ((Result<byte[], AeroError>.Ok)mediaBytesResult).Value;

                var fileName = media.Path.Split('/').Last();
                var uploadContent = new MultipartFormDataContent
                {
                    { new ByteArrayContent(mediaBytes), "video_file", fileName }
                };

                await client.PostAsync(uploadServerData.Response.UploadUrl, uploadContent, cancellationToken);

                mediaList.Add(new VkMedia
                {
                    Id = uploadServerData.Response.VideoId,
                    Type = "video"
                });
            }
            else
            {
                var uploadServerRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.vk.com/method/photos.getWallUploadServer?owner_id={userId}&access_token={accessToken}&v=5.251");
                var uploadServerResponse = await client.SendAsync(uploadServerRequest, cancellationToken);
                var uploadServerData = await DeserializeAsync<VkPhotoUploadServerResponse>(uploadServerResponse);

                var mediaBytesResult = await ReadOrFetchAsync(media.Path, cancellationToken);
                if (mediaBytesResult is Result<byte[], AeroError>.Failure failure) return failure.Error;
                var mediaBytes = ((Result<byte[], AeroError>.Ok)mediaBytesResult).Value;

                var fileName = media.Path.Split('/').Last();
                var uploadContent = new MultipartFormDataContent
                {
                    { new ByteArrayContent(mediaBytes), "photo", fileName }
                };

                var uploadResponse = await client.PostAsync(uploadServerData.Response.UploadUrl, uploadContent, cancellationToken);
                var uploadResult = await DeserializeAsync<VkPhotoUploadResult>(uploadResponse);

                var saveFormData = new MultipartFormDataContent
                {
                    { new StringContent(uploadResult.Photo), "photo" },
                    { new StringContent(uploadResult.Server.ToString()), "server" },
                    { new StringContent(uploadResult.Hash), "hash" }
                };

                var saveRequest = new HttpRequestMessage(HttpMethod.Post, $"https://api.vk.com/method/photos.saveWallPhoto?access_token={accessToken}&v=5.251")
                {
                    Content = saveFormData
                };

                var saveResponse = await client.SendAsync(saveRequest, cancellationToken);
                var saveResult = await DeserializeAsync<VkSavePhotoResponse>(saveResponse);

                if (saveResult.Response != null && saveResult.Response.Count > 0)
                {
                    mediaList.Add(new VkMedia
                    {
                        Id = saveResult.Response[0].Id.ToString(),
                        Type = "photo"
                    });
                }
            }
        }

        return mediaList;
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = Encoding.UTF8.GetBytes(codeVerifier);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private string GetClientId() => configuration["VK_ID"] ?? throw new InvalidOperationException("VK_ID not configured");
    private string GetFrontendUrl() => configuration["FRONTEND_URL"] ?? throw new InvalidOperationException("FRONTEND_URL not configured");

    //#region DTOs

    private class VkMedia
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
public string Id { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Type.
        /// </summary>
public string Type { get; set; } = string.Empty;
    }

    private class VkTokenResponse
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
        public string? Scope { get; set; }
    }

    private class VkUserInfoResponse
    {
                /// <summary>
        /// Gets or sets the User.
        /// </summary>
[JsonPropertyName("user")]
        public VkUserInfo User { get; set; } = new();
    }

    private class VkUserInfo
    {
                /// <summary>
        /// Gets or sets the User Id.
        /// </summary>
[JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the First Name.
        /// </summary>
[JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Last Name.
        /// </summary>
[JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Avatar.
        /// </summary>
[JsonPropertyName("avatar")]
        public string? Avatar { get; set; }
    }

    private class VkWallPostResponse
    {
                /// <summary>
        /// Gets or sets the Response.
        /// </summary>
[JsonPropertyName("response")]
        public VkWallPostResponseData? Response { get; set; }
    }

    private class VkWallPostResponseData
    {
                /// <summary>
        /// Gets or sets the Post Id.
        /// </summary>
[JsonPropertyName("post_id")]
        public int PostId { get; set; }
    }

    private class VkCommentResponse
    {
                /// <summary>
        /// Gets or sets the Response.
        /// </summary>
[JsonPropertyName("response")]
        public VkCommentResponseData? Response { get; set; }
    }

    private class VkCommentResponseData
    {
                /// <summary>
        /// Gets or sets the Comment Id.
        /// </summary>
[JsonPropertyName("comment_id")]
        public int CommentId { get; set; }
    }

    private class VkPhotoUploadServerResponse
    {
                /// <summary>
        /// Gets or sets the Response.
        /// </summary>
[JsonPropertyName("response")]
        public VkUploadServerData Response { get; set; } = new();
    }

    private class VkVideoUploadServerResponse
    {
                /// <summary>
        /// Gets or sets the Response.
        /// </summary>
[JsonPropertyName("response")]
        public VkVideoUploadData Response { get; set; } = new();
    }

    private class VkUploadServerData
    {
                /// <summary>
        /// Gets or sets the Upload Url.
        /// </summary>
[JsonPropertyName("upload_url")]
        public string UploadUrl { get; set; } = string.Empty;
    }

    private class VkVideoUploadData
    {
                /// <summary>
        /// Gets or sets the Upload Url.
        /// </summary>
[JsonPropertyName("upload_url")]
        public string UploadUrl { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Video Id.
        /// </summary>
[JsonPropertyName("video_id")]
        public string VideoId { get; set; } = string.Empty;
    }

    private class VkPhotoUploadResult
    {
                /// <summary>
        /// Gets or sets the Photo.
        /// </summary>
[JsonPropertyName("photo")]
        public string Photo { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Server.
        /// </summary>
[JsonPropertyName("server")]
        public int Server { get; set; }

                /// <summary>
        /// Gets or sets the Hash.
        /// </summary>
[JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;
    }

    private class VkSavePhotoResponse
    {
                /// <summary>
        /// Gets or sets the Response.
        /// </summary>
[JsonPropertyName("response")]
        public List<VkSavedPhoto>? Response { get; set; }
    }

    private class VkSavedPhoto
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public int Id { get; set; }
    }

    //#endregion
}
