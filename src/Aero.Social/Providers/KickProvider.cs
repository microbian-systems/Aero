using System.Security.Cryptography;
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
/// Represents a class for KickProvider.
/// </summary>
public class KickProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<KickProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "kick";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "Kick";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => new[] { "chat:write", "user:read", "channel:read" };
        /// <summary>
    /// Gets or sets the Max Concurrent Jobs.
    /// </summary>
public override int MaxConcurrentJobs => 3;

        /// <summary>
    /// MaxLength method.
    /// </summary>
public override int MaxLength(object? additionalSettings = null) => 500;

        /// <summary>
    /// GenerateAuthUrlAsync method.
    /// </summary>
public override async Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(32);
        var (codeVerifier, codeChallenge) = GeneratePKCE();

        var clientId = GetClientId();
        var frontendUrl = GetFrontendUrl();
        var redirectUri = $"{frontendUrl}/integrations/social/kick";

        var url = "https://id.kick.com/oauth/authorize" +
                  $"?response_type=code" +
                  $"&client_id={clientId}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&scope={Uri.EscapeDataString(string.Join(" ", Scopes))}" +
                  $"&state={state}" +
                  $"&code_challenge={codeChallenge}" +
                  $"&code_challenge_method=S256";

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
        var redirectUri = $"{frontendUrl}/integrations/social/kick{(parameters.Refresh != null ? $"?refresh={parameters.Refresh}" : "")}";

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = redirectUri,
            ["code"] = parameters.Code,
            ["code_verifier"] = parameters.CodeVerifier ?? ""
        };

        var response = await client.PostAsync("https://id.kick.com/oauth/token", new FormUrlEncodedContent(form), cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenInfo = await DeserializeAsync<KickTokenResponse>(response);
        var userInfo = await GetUserInfoAsync(tokenInfo.AccessToken, cancellationToken);

        return new AuthTokenDetails
        {
            Id = userInfo.Id,
            Name = userInfo.Name,
            AccessToken = tokenInfo.AccessToken,
            RefreshToken = tokenInfo.RefreshToken,
            ExpiresIn = tokenInfo.ExpiresIn,
            Picture = userInfo.Picture ?? string.Empty,
            Username = userInfo.Username
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

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["refresh_token"] = refreshToken
        };

        var response = await client.PostAsync("https://id.kick.com/oauth/token", new FormUrlEncodedContent(form), cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenInfo = await DeserializeAsync<KickTokenResponse>(response);
        var userInfo = await GetUserInfoAsync(tokenInfo.AccessToken, cancellationToken);

        return new AuthTokenDetails
        {
            Id = userInfo.Id,
            Name = userInfo.Name,
            AccessToken = tokenInfo.AccessToken,
            RefreshToken = tokenInfo.RefreshToken,
            ExpiresIn = tokenInfo.ExpiresIn,
            Picture = userInfo.Picture ?? string.Empty,
            Username = userInfo.Username
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
        if (posts.Count == 0)
            return Array.Empty<PostResponse>();

        var firstPost = posts[0];

        var payload = new
        {
            type = "user",
            content = firstPost.Message.Substring(0, Math.Min(firstPost.Message.Length, 500)),
            broadcaster_user_id = int.Parse(id)
        };

        var request = CreateJsonRequest("https://api.kick.com/public/v1/chat", HttpMethod.Post, payload, accessToken);

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var chatResponse = await DeserializeAsync<KickChatResponse>(response);

        return new[]
        {
            new PostResponse
            {
                Id = firstPost.Id,
                PostId = chatResponse.Data?.MessageId ?? MakeId(10),
                ReleaseUrl = $"https://kick.com/{integration.Username ?? "channel"}",
                Status = chatResponse.Data?.IsSent == true ? "posted" : "error"
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
        if (posts.Count == 0)
            return Array.Empty<PostResponse>();

        var commentPost = posts[0];

        var payload = new Dictionary<string, object?>
        {
            ["type"] = "user",
            ["content"] = commentPost.Message.Substring(0, Math.Min(commentPost.Message.Length, 500)),
            ["broadcaster_user_id"] = int.Parse(id),
            ["reply_to_message_id"] = lastCommentId ?? postId
        };

        var request = CreateJsonRequest("https://api.kick.com/public/v1/chat", HttpMethod.Post, payload, accessToken);

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var chatResponse = await DeserializeAsync<KickChatResponse>(response);

        return new[]
        {
            new PostResponse
            {
                Id = commentPost.Id,
                PostId = chatResponse.Data?.MessageId ?? MakeId(10),
                ReleaseUrl = $"https://kick.com/{integration.Username ?? "channel"}",
                Status = chatResponse.Data?.IsSent == true ? "posted" : "error"
            }
        };
    }

    private async Task<KickUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.kick.com/public/v1/users");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var userResponse = await DeserializeAsync<KickUserResponse>(response);
        var user = userResponse.Data?.FirstOrDefault();
        if (user == null) return new KickUserInfo { Id = string.Empty, Name = string.Empty, Username = string.Empty, Picture = null };

        return new KickUserInfo
        {
            Id = user.UserId ?? user.Id?.ToString() ?? "",
            Name = user.Name ?? "",
            Username = user.Name ?? "",
            Picture = user.ProfilePicture
        };
    }

    private static (string CodeVerifier, string CodeChallenge) GeneratePKCE()
    {
        var codeVerifier = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        var codeChallenge = Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return (codeVerifier, codeChallenge);
    }

    private static HttpRequestMessage CreateJsonRequest(string url, HttpMethod method, object? payload, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);

        if (payload != null)
        {
            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        return request;
    }

    private string GetClientId() => configuration["KICK_CLIENT_ID"] ?? string.Empty;
    private string GetClientSecret() => configuration["KICK_SECRET"] ?? string.Empty;
    private string GetFrontendUrl() => configuration["FRONTEND_URL"] ?? string.Empty;

    //#region DTOs

    private class KickTokenResponse
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

    private class KickUserResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public List<KickUser>? Data { get; set; }
    }

    private class KickUser
    {
                /// <summary>
        /// Gets or sets the User Id.
        /// </summary>
[JsonPropertyName("user_id")]
        public string? UserId { get; set; }

                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public int? Id { get; set; }

                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
[JsonPropertyName("name")]
        public string? Name { get; set; }

                /// <summary>
        /// Gets or sets the Profile Picture.
        /// </summary>
[JsonPropertyName("profile_picture")]
        public string? ProfilePicture { get; set; }
    }

    private class KickUserInfo
    {
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
public string? Picture { get; set; }
    }

    private class KickChatResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public KickChatData? Data { get; set; }
    }

    private class KickChatData
    {
                /// <summary>
        /// Gets or sets the Message Id.
        /// </summary>
[JsonPropertyName("message_id")]
        public string? MessageId { get; set; }

                /// <summary>
        /// Gets or sets the Is Sent.
        /// </summary>
[JsonPropertyName("is_sent")]
        public bool IsSent { get; set; }
    }

    //#endregion
}
