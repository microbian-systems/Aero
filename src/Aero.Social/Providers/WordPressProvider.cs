using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Aero.Core;
using Aero.Core.Railway;
using Aero.Social.Abstractions;
using Aero.Social.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Providers;

/// <summary>
/// Represents a class for WordPressProvider.
/// </summary>
public class WordPressProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<WordPressProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    private readonly IConfiguration _configuration = configuration;

        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "wordpress";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "WordPress";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => Array.Empty<string>();
        /// <summary>
    /// Gets or sets the Max Concurrent Jobs.
    /// </summary>
public override int MaxConcurrentJobs => 5;
        /// <summary>
    /// Gets or sets the Editor.
    /// </summary>
public override EditorType Editor => EditorType.Html;

        /// <summary>
    /// MaxLength method.
    /// </summary>
public override int MaxLength(object? additionalSettings = null) => 100000;

        /// <summary>
    /// GenerateAuthUrlAsync method.
    /// </summary>
public override async Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(6);
        return new GenerateAuthUrlResponse
        {
            Url = "",
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
        var bodyBytes = Convert.FromBase64String(parameters.Code);
        var bodyJson = Encoding.UTF8.GetString(bodyBytes);
        var authBody = JsonSerializer.Deserialize<WordPressAuthBody>(bodyJson);
        if (authBody == null) return AeroError.CreateError("Invalid auth body");

        try
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{authBody.Username}:{authBody.Password}"));

            var request = new HttpRequestMessage(HttpMethod.Get, $"{authBody.Domain}/wp-json/wp/v2/users/me");
            request.Headers.TryAddWithoutValidation("Authorization", $"Basic {auth}");

            var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return AeroError.CreateError("Invalid credentials");
            }

            var userInfo = await DeserializeAsync<WordPressUserInfo>(response);

            string? picture = null;
            if (userInfo.AvatarUrls != null && userInfo.AvatarUrls.Count > 0)
            {
                var biggestSize = userInfo.AvatarUrls.Keys
                    .Select(k => int.TryParse(k, out var size) ? size : 0)
                    .Max();

                picture = userInfo.AvatarUrls.GetValueOrDefault(biggestSize.ToString())
                       ?? userInfo.AvatarUrls.Values.LastOrDefault();
            }

            return new AuthTokenDetails
            {
                RefreshToken = "",
                ExpiresIn = (int)TimeSpan.FromDays(100).TotalSeconds,
                AccessToken = parameters.Code,
                Id = $"{authBody.Domain}_{userInfo.Id}",
                Name = userInfo.Name ?? "",
                Picture = picture ?? string.Empty,
                Username = authBody.Username
            };
        }
        catch (Exception)
        {
            return AeroError.CreateError("Invalid credentials");
        }
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
            RefreshToken = "",
            ExpiresIn = 0,
            AccessToken = "",
            Id = "",
            Name = "",
            Picture = "",
            Username = ""
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
        if (posts.Count == 0)
            return Array.Empty<PostResponse>();

        var bodyBytes = Convert.FromBase64String(accessToken);
        var bodyJson = Encoding.UTF8.GetString(bodyBytes);
        var authBody = JsonSerializer.Deserialize<WordPressAuthBody>(bodyJson);
        if (authBody == null) return AeroError.CreateError("Invalid auth body");

        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{authBody.Username}:{authBody.Password}"));

        var firstPost = posts[0];
        var settings = firstPost.Settings ?? new Dictionary<string, object>();

        var title = GetSettingValue<string>(settings, "title") ?? "";
        var postType = GetSettingValue<string>(settings, "type") ?? "posts";
        var mainImage = GetSettingValue<MediaContent>(settings, "main_image");

        int? mediaId = null;

        if (mainImage?.Path != null)
        {
            mediaId = await UploadMediaAsync(authBody, auth, mainImage.Path, cancellationToken);
        }

        var slug = GenerateSlug(title);

        var payload = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["content"] = firstPost.Message,
            ["slug"] = slug,
            ["status"] = "publish"
        };

        if (mediaId.HasValue)
        {
            payload["featured_media"] = mediaId.Value;
        }

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{authBody.Domain}/wp-json/wp/v2/{postType}") { Content = content };
        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {auth}");

            var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var postResponse = await DeserializeAsync<WordPressPostResponse>(response);

        return new[]
        {
            new PostResponse
            {
                Id = firstPost.Id,
                Status = "completed",
                PostId = postResponse.Id.ToString(),
                ReleaseUrl = postResponse.Link ?? ""
            }
        };
    }

        /// <summary>
    /// GetPostTypesAsync method.
    /// </summary>
public async Task<List<WordPressPostType>> GetPostTypesAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var bodyBytes = Convert.FromBase64String(accessToken);
        var bodyJson = Encoding.UTF8.GetString(bodyBytes);
        var authBody = JsonSerializer.Deserialize<WordPressAuthBody>(bodyJson);
        if (authBody == null) return new List<WordPressPostType>();

        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{authBody.Username}:{authBody.Password}"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"{authBody.Domain}/wp-json/wp/v2/types");
        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {auth}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var postTypesDict = JsonSerializer.Deserialize<Dictionary<string, WordPressPostTypeRaw>>(rawJson);

        var result = new List<WordPressPostType>();

        if (postTypesDict != null)
        {
            foreach (var kvp in postTypesDict)
            {
                if (kvp.Key.StartsWith("wp_") || kvp.Key.StartsWith("nav_") || kvp.Key == "attachment")
                    continue;

                result.Add(new WordPressPostType
                {
                    Id = kvp.Value.RestBase ?? kvp.Key,
                    Name = kvp.Value.Name ?? kvp.Key
                });
            }
        }

        return result;
    }

    private async Task<int?> UploadMediaAsync(
        WordPressAuthBody authBody,
        string auth,
        string imageUrl,
        CancellationToken cancellationToken)
    {
        byte[] imageBytes;
        string fileName;

        if (imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            imageBytes = await client.GetByteArrayAsync(imageUrl, cancellationToken);
            fileName = imageUrl.Split('/').Last() ?? "image.jpg";
        }
        else
        {
            imageBytes = await File.ReadAllBytesAsync(imageUrl, cancellationToken);
            fileName = Path.GetFileName(imageUrl) ?? "image.jpg";
        }

        var contentType = GetContentType(fileName);

        var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{authBody.Domain}/wp-json/wp/v2/media") { Content = content };
        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {auth}");
        request.Headers.TryAddWithoutValidation("Content-Disposition", $"attachment; filename=\"{fileName}\"");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var mediaResponse = await DeserializeAsync<WordPressMediaResponse>(response);
        return mediaResponse.Id;
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-").Trim();
        slug = Regex.Replace(slug, @"-+", "-");
        return slug;
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
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

    //#region DTOs

    private class WordPressAuthBody
    {
                /// <summary>
        /// Gets or sets the Domain.
        /// </summary>
[JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Username.
        /// </summary>
[JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Password.
        /// </summary>
[JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    private class WordPressUserInfo
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public int Id { get; set; }

                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
[JsonPropertyName("name")]
        public string? Name { get; set; }

                /// <summary>
        /// Gets or sets the Avatar Urls.
        /// </summary>
[JsonPropertyName("avatar_urls")]
        public Dictionary<string, string>? AvatarUrls { get; set; }
    }

    private class WordPressPostResponse
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public int Id { get; set; }

                /// <summary>
        /// Gets or sets the Link.
        /// </summary>
[JsonPropertyName("link")]
        public string? Link { get; set; }
    }

    private class WordPressMediaResponse
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public int Id { get; set; }
    }

    private class WordPressPostTypeRaw
    {
                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
[JsonPropertyName("name")]
        public string? Name { get; set; }

                /// <summary>
        /// Gets or sets the Rest Base.
        /// </summary>
[JsonPropertyName("rest_base")]
        public string? RestBase { get; set; }
    }

        /// <summary>
    /// Represents a class for WordPressPostType.
    /// </summary>
public class WordPressPostType
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
public string Id { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
public string Name { get; set; } = string.Empty;
    }

    //#endregion
}
