using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aero.Core;
using Aero.Core.Railway;
using Aero.Social.Abstractions;
using Aero.Social.Models;
using Microsoft.Extensions.Logging;

namespace Aero.Social.Providers;

/// <summary>
/// Provides integration with the Dev.to social media platform.
/// </summary>
/// <param name="httpClient">The HTTP client for making API requests.</param>
/// <param name="logger">The logger for this provider.</param>
public class DevToProvider(
    HttpClient httpClient,
    ILogger<DevToProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    /// <inheritdoc/>
    public override string Identifier => "devto";

    /// <inheritdoc/>
    public override string Name => "Dev.to";

    /// <inheritdoc/>
    public override string[] Scopes => Array.Empty<string>();

    /// <inheritdoc/>
    public override int MaxConcurrentJobs => 3;

    /// <inheritdoc/>
    public override EditorType Editor => EditorType.Markdown;

    /// <inheritdoc/>
    public override int MaxLength(object? additionalSettings = null) => 100000;

    /// <inheritdoc/>
    protected override ErrorHandlingResult? HandleErrors(string responseBody)
    {
        if (responseBody.Contains("Canonical url has already been taken"))
        {
            return new ErrorHandlingResult(ErrorHandlingType.BadBody, "Canonical URL already exists");
        }

        return null;
    }

    /// <inheritdoc/>
    public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(6);
        return Task.FromResult<Result<GenerateAuthUrlResponse, AeroError>>(new GenerateAuthUrlResponse
        {
            Url = "",
            CodeVerifier = MakeId(10),
            State = state
        });
    }

    /// <inheritdoc/>
    public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        byte[] bodyBytes;
        try
        {
            bodyBytes = Convert.FromBase64String(parameters.Code);
        }
        catch (Exception ex)
        {
            return AeroError.ValidationError([$"Invalid auth code: {ex.Message}"]);
        }

        DevToAuthBody? authBody;
        try
        {
            var bodyJson = Encoding.UTF8.GetString(bodyBytes);
            authBody = JsonSerializer.Deserialize<DevToAuthBody>(bodyJson);
        }
        catch (Exception ex)
        {
            return AeroError.ValidationError([$"Failed to parse auth body: {ex.Message}"]);
        }

        if (authBody == null || string.IsNullOrEmpty(authBody.ApiKey))
        {
            return AeroError.ValidationError(["Invalid auth body or missing ApiKey"]);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "https://dev.to/api/users/me");
        request.Headers.TryAddWithoutValidation("api-key", authBody.ApiKey);

        var userInfoResult = await SendRequestAsync<DevToUserInfo>(request, cancellationToken);
        if (userInfoResult is Result<DevToUserInfo, AeroError>.Failure userInfoFailure)
        {
            return userInfoFailure.Error;
        }

        var userInfo = ((Result<DevToUserInfo, AeroError>.Ok)userInfoResult).Value;
        return new AuthTokenDetails
        {
            RefreshToken = "",
            ExpiresIn = (int)TimeSpan.FromDays(100).TotalSeconds,
            AccessToken = authBody.ApiKey,
            Id = userInfo.Id.ToString(),
            Name = userInfo.Name ?? "",
            Picture = userInfo.ProfileImage ?? string.Empty,
            Username = userInfo.Username ?? ""
        };
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
        var settings = firstPost.Settings ?? new Dictionary<string, object>();

        var title = GetSettingValue<string>(settings, "title") ?? "";
        var mainImage = GetSettingValue<MediaContent>(settings, "main_image");
        var tags = GetSettingValue<List<DevToTag>>(settings, "tags") ?? new List<DevToTag>();
        var organization = GetSettingValue<string>(settings, "organization");
        var canonical = GetSettingValue<string>(settings, "canonical");

        var article = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["body_markdown"] = firstPost.Message,
            ["published"] = true
        };

        if (mainImage?.Path != null)
        {
            article["main_image"] = mainImage.Path;
        }

        if (tags.Count > 0)
        {
            article["tags"] = tags.Select(t => t.Label).ToArray();
        }

        if (!string.IsNullOrEmpty(organization))
        {
            article["organization_id"] = organization;
        }

        if (!string.IsNullOrEmpty(canonical))
        {
            article["canonical_url"] = canonical;
        }

        var payload = new { article };
        var request = CreateRequest("https://dev.to/api/articles", HttpMethod.Post, payload);
        request.Headers.TryAddWithoutValidation("api-key", accessToken);

        return await SendRequestAsync<DevToArticleResponse>(request, cancellationToken)
            .MapAsync<DevToArticleResponse, AeroError, PostResponse[]>(articleResponse => new[]
            {
                new PostResponse
                {
                    Id = firstPost.Id,
                    Status = "completed",
                    PostId = articleResponse.Id.ToString(),
                    ReleaseUrl = articleResponse.Url ?? ""
                }
            });
    }

    /// <summary>
    /// Retrieves tags from the Dev.to API.
    /// </summary>
    /// <param name="accessToken">The API key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of tags.</returns>
    public async Task<Result<List<DevToTag>, AeroError>> GetTagsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://dev.to/api/tags?per_page=1000&page=1");
        request.Headers.TryAddWithoutValidation("api-key", accessToken);

        return await SendRequestAsync<List<DevToTagResponse>>(request, cancellationToken)
            .MapAsync<List<DevToTagResponse>, AeroError, List<DevToTag>>(tags =>
                tags.Select(t => new DevToTag { Value = t.Id, Label = t.Name }).ToList());
    }

    /// <summary>
    /// Retrieves organizations the user belongs to from the Dev.to API.
    /// </summary>
    /// <param name="accessToken">The API key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of organizations.</returns>
    public async Task<Result<List<DevToOrganization>, AeroError>> GetOrganizationsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://dev.to/api/articles/me/all?per_page=1000");
        request.Headers.TryAddWithoutValidation("api-key", accessToken);

        return await SendRequestAsync<List<DevToArticleItem>>(request, cancellationToken)
            .BindAsync<List<DevToArticleItem>, AeroError, List<DevToOrganization>>(async articles =>
            {
                var orgUsernames = articles
                    .Where(a => a.Organization?.Username != null)
                    .Select(a => a.Organization!.Username!)
                    .Distinct()
                    .ToList();

                var organizations = new List<DevToOrganization>();

                foreach (var orgUsername in orgUsernames)
                {
                    var orgRequest = new HttpRequestMessage(HttpMethod.Get, $"https://dev.to/api/organizations/{orgUsername}");
                    orgRequest.Headers.TryAddWithoutValidation("api-key", accessToken);

                    var orgResult = await SendRequestAsync<DevToOrganizationResponse>(orgRequest, cancellationToken);
                    if (orgResult is Result<DevToOrganizationResponse, AeroError>.Ok ok)
                    {
                        organizations.Add(new DevToOrganization
                        {
                            Id = ok.Value.Id.ToString(),
                            Name = ok.Value.Name ?? "",
                            Username = ok.Value.Username ?? ""
                        });
                    }
                }

                return organizations;
            });
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

    private class DevToAuthBody
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = string.Empty;
    }

    private class DevToUserInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("profile_image")]
        public string? ProfileImage { get; set; }
    }

    private class DevToArticleResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    private class DevToTagResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    private class DevToArticleItem
    {
        [JsonPropertyName("organization")]
        public DevToOrganizationRef? Organization { get; set; }
    }

    private class DevToOrganizationRef
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }

    private class DevToOrganizationResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }

    /// <summary>
    /// Represents a Dev.to tag.
    /// </summary>
    public class DevToTag
    {
        /// <summary>
        /// Gets or sets the tag value (ID).
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tag label (Name).
        /// </summary>
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a Dev.to organization.
    /// </summary>
    public class DevToOrganization
    {
        /// <summary>
        /// Gets or sets the organization ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the organization name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the organization username.
        /// </summary>
        public string Username { get; set; } = string.Empty;
    }
}

