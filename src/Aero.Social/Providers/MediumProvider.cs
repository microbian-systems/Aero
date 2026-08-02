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
/// Represents a class for MediumProvider.
/// </summary>
public class MediumProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<MediumProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    private readonly IConfiguration _configuration = configuration;

        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "medium";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "Medium";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => Array.Empty<string>();
        /// <summary>
    /// Gets or sets the Editor.
    /// </summary>
public override EditorType Editor => EditorType.Markdown;
        /// <summary>
    /// Gets or sets the Max Concurrent Jobs.
    /// </summary>
public override int MaxConcurrentJobs => 3;

        /// <summary>
    /// MaxLength method.
    /// </summary>
public override int MaxLength(object? additionalSettings = null) => 100000;

        /// <summary>
    /// GenerateAuthUrlAsync method.
    /// </summary>
public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(6);
        return Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(new GenerateAuthUrlResponse
        {
            Url = string.Empty,
            CodeVerifier = MakeId(10),
            State = state
        });
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
    /// AuthenticateAsync method.
    /// </summary>
public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = Convert.FromBase64String(parameters.Code);
        var json = Encoding.UTF8.GetString(bytes);
        var body = JsonSerializer.Deserialize<MediumAuthBody>(json);

        if (body?.ApiKey == null)
        {
            return AeroError.BadRequestError("Invalid credentials");
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.medium.com/v1/me");
            request.Headers.Add("Authorization", $"Bearer {body.ApiKey}");

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var userResponse = await DeserializeAsync<MediumUserResponse>(response);

            return new AuthTokenDetails
            {
                RefreshToken = string.Empty,
                ExpiresIn = (int)TimeSpan.FromDays(100 * 365).TotalSeconds,
                AccessToken = body.ApiKey,
                Id = userResponse.Data.Id,
                Name = userResponse.Data.Name,
                Picture = userResponse.Data.ImageUrl ?? string.Empty,
                Username = userResponse.Data.Username
            };
        }
        catch
        {
            return AeroError.BadRequestError("Invalid credentials");
        }
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

        var publicationId = GetSettingValue<string>(settings, "publication");
        var title = GetSettingValue<string>(settings, "title") ?? "Untitled";
        var canonicalUrl = GetSettingValue<string>(settings, "canonical");
        var tags = GetSettingValue<List<string>>(settings, "tags");
        var publishStatus = !string.IsNullOrEmpty(publicationId) ? "draft" : "public";

        var payload = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["contentFormat"] = "markdown",
            ["content"] = firstPost.Message,
            ["publishStatus"] = publishStatus
        };

        if (!string.IsNullOrEmpty(canonicalUrl))
        {
            payload["canonicalUrl"] = canonicalUrl;
        }

        if (tags != null && tags.Count > 0)
        {
            payload["tags"] = tags;
        }

        var url = !string.IsNullOrEmpty(publicationId)
            ? $"https://api.medium.com/v1/publications/{publicationId}/posts"
            : $"https://api.medium.com/v1/users/{id}/posts";

        var request = CreateRequest(url, HttpMethod.Post, payload);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var postResponse = await DeserializeAsync<MediumPostResponse>(response);

        return new[]
        {
            new PostResponse
            {
                Id = firstPost.Id,
                Status = "completed",
                PostId = postResponse.Data.Id,
                ReleaseUrl = postResponse.Data.Url
            }
        };
    }

        /// <summary>
    /// GetPublicationsAsync method.
    /// </summary>
public async Task<List<MediumPublication>> GetPublicationsAsync(string accessToken, string userId, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.medium.com/v1/users/{userId}/publications");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var publicationsResponse = await DeserializeAsync<MediumPublicationsResponse>(response);
        return publicationsResponse.Data ?? new List<MediumPublication>();
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

    private class MediumAuthBody
    {
                /// <summary>
        /// Gets or sets the Api Key.
        /// </summary>
[JsonPropertyName("apiKey")]
        public string? ApiKey { get; set; }
    }

    //#region DTOs

    private class MediumUserResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public MediumUserData Data { get; set; } = new();
    }

    private class MediumUserData
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
        /// Gets or sets the Username.
        /// </summary>
[JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Image Url.
        /// </summary>
[JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }
    }

    private class MediumPostResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public MediumPostData Data { get; set; } = new();
    }

    private class MediumPostData
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Url.
        /// </summary>
[JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    private class MediumPublicationsResponse
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public List<MediumPublication>? Data { get; set; }
    }

        /// <summary>
    /// Represents a class for MediumPublication.
    /// </summary>
public class MediumPublication
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
        /// Gets or sets the Url.
        /// </summary>
[JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    //#endregion
}
