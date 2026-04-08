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
/// Provides integration with Dribbble for authenticating users and posting shots.
/// </summary>
/// <param name="httpClient">The HTTP client instance.</param>
/// <param name="configuration">The configuration instance.</param>
/// <param name="logger">The logger instance.</param>
public class DribbbleProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<DribbbleProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    /// <inheritdoc/>
    public override string Identifier => "dribbble";

    /// <inheritdoc/>
    public override string Name => "Dribbble";

    /// <inheritdoc/>
    public override string[] Scopes => ["public", "upload"];

    /// <inheritdoc/>
    public override int MaxConcurrentJobs => 3;

    /// <inheritdoc/>
    public override int MaxLength(object? additionalSettings = null) => 40000;

    /// <inheritdoc/>
    public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(6);

        return Task.FromResult(GetClientId().Bind(clientId =>
            GetFrontendUrl().Map(frontendUrl =>
            {
                var redirectUri = $"{frontendUrl}/integrations/social/dribbble";
                var url = "https://dribbble.com/oauth/authorize" +
                          $"?client_id={clientId}" +
                          $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                          $"&response_type=code" +
                          $"&scope={string.Join("+", Scopes)}" +
                          $"&state={state}";

                return new GenerateAuthUrlResponse
                {
                    Url = url,
                    CodeVerifier = MakeId(10),
                    State = state
                };
            })));
    }

    /// <inheritdoc/>
    public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        return await GetClientId().BindAsync(async clientId =>
            await GetClientSecret().BindAsync(async clientSecret =>
            await GetFrontendUrl().BindAsync(async frontendUrl =>
            {
                var tokenUrl = $"https://dribbble.com/oauth/token" +
                               $"?client_id={clientId}" +
                               $"&client_secret={clientSecret}" +
                               $"&code={parameters.Code}" +
                               $"&redirect_uri={Uri.EscapeDataString($"{frontendUrl}/integrations/social/dribbble")}";

                var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
                return await SendRequestAsync<DribbbleTokenResponse>(request, cancellationToken);
            })))
            .BindAsync(async tokenInfo =>
            {
                var scopeCheck = CheckScopes(Scopes, tokenInfo.Scope ?? "");
                if (scopeCheck is Result<NoneType, AeroError>.Failure failure)
                {
                    return failure.Error;
                }

                return await GetUserInfoAsync(tokenInfo.AccessToken, cancellationToken)
                    .MapAsync(userInfo => new AuthTokenDetails
                    {
                        Id = userInfo.Id.ToString(),
                        Name = userInfo.Name ?? "",
                        AccessToken = tokenInfo.AccessToken,
                        RefreshToken = string.Empty,
                        ExpiresIn = 999999999,
                        Picture = userInfo.AvatarUrl ?? string.Empty,
                        Username = userInfo.Login ?? ""
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
        if (posts.Count == 0 || posts[0].Media == null || posts[0].Media.Count == 0)
        {
            return Array.Empty<PostResponse>();
        }

        var firstPost = posts[0];
        var media = firstPost.Media[0];
        var settings = firstPost.Settings ?? new Dictionary<string, object>();
        var title = GetSettingValue<string>(settings, "title") ?? string.Empty;

        return await ReadOrFetchAsync(media.Path, cancellationToken)
            .BindAsync(async imageBytes =>
            {
                var fileName = GetFileName(media.Path);
                var contentType = GetContentType(fileName);

                using var formData = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                
                formData.Add(imageContent, "image", fileName);
                formData.Add(new StringContent(title), "title");
                formData.Add(new StringContent(firstPost.Message), "description");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.dribbble.com/v2/shots")
                {
                    Content = formData
                };
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");

                return await SendRequestAsync(request, cancellationToken)
                    .MapAsync(response =>
                    {
                        var location = response.Headers.Location?.ToString() ?? string.Empty;
                        var newId = location.Split('/').Last() ?? string.Empty;

                        return new[]
                        {
                            new PostResponse
                            {
                                Id = firstPost.Id,
                                Status = "completed",
                                PostId = newId,
                                ReleaseUrl = $"https://dribbble.com/shots/{newId}"
                            }
                        };
                    });
            });
    }

    /// <summary>
    /// Retrieves teams the user belongs to from the Dribbble API.
    /// </summary>
    /// <param name="accessToken">The user access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of teams.</returns>
    public async Task<Result<List<DribbbleTeam>, AeroError>> GetTeamsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.dribbble.com/v2/user");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");

        return await SendRequestAsync<DribbbleUserWithTeams>(request, cancellationToken)
            .MapAsync(userResponse => userResponse.Teams?.Select(t => new DribbbleTeam
            {
                Id = t.Id.ToString(),
                Name = t.Name ?? string.Empty
            }).ToList() ?? []);
    }

    private async Task<Result<DribbbleUserInfo, AeroError>> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.dribbble.com/v2/user");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");

        return await SendRequestAsync<DribbbleUserInfo>(request, cancellationToken);
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

    private static string GetFileName(string path)
    {
        return path.Split('/').Last() ?? "image.png";
    }

    private static T? GetSettingValue<T>(Dictionary<string, object> settings, string key)
    {
        if (!settings.TryGetValue(key, out var value))
        {
            return default;
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        var json = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<T>(json);
    }

    private Result<string, AeroError> GetClientId() => 
        configuration["DRIBBBLE_CLIENT_ID"] is { } clientId 
            ? clientId 
            : AeroError.CreateError("DRIBBBLE_CLIENT_ID not configured");

    private Result<string, AeroError> GetClientSecret() => 
        configuration["DRIBBBLE_CLIENT_SECRET"] is { } clientSecret 
            ? clientSecret 
            : AeroError.CreateError("DRIBBBLE_CLIENT_SECRET not configured");

    private Result<string, AeroError> GetFrontendUrl() => 
        configuration["FRONTEND_URL"] is { } frontendUrl 
            ? frontendUrl 
            : AeroError.CreateError("FRONTEND_URL not configured");

    //#region DTOs

    private class DribbbleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }

    private class DribbbleUserInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("login")]
        public string? Login { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }
    }

    private class DribbbleUserWithTeams : DribbbleUserInfo
    {
        [JsonPropertyName("teams")]
        public List<DribbbleTeamInfo>? Teams { get; set; }
    }

    private class DribbbleTeamInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    /// <summary>
    /// Represents a Dribbble team.
    /// </summary>
    public class DribbbleTeam
    {
        /// <summary>
        /// Gets or sets the team ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the team name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    //#endregion
}
