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
/// Represents a class for HashnodeProvider.
/// </summary>
public class HashnodeProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<HashnodeProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    private const string GraphQLEndpoint = "https://gql.hashnode.com";

        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "hashnode";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "Hashnode";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => Array.Empty<string>();
        /// <summary>
    /// Gets or sets the Max Concurrent Jobs.
    /// </summary>
public override int MaxConcurrentJobs => 3;
        /// <summary>
    /// Gets or sets the Editor.
    /// </summary>
public override EditorType Editor => EditorType.Markdown;

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
        HashnodeAuthBody? authBody;
        try
        {
            var bodyBytes = Convert.FromBase64String(parameters.Code);
            var bodyJson = Encoding.UTF8.GetString(bodyBytes);
            authBody = JsonSerializer.Deserialize<HashnodeAuthBody>(bodyJson);
        }
        catch (Exception ex)
        {
            return AeroError.ValidationError([$"Invalid auth body: {ex.Message}"]);
        }

        if (authBody == null || string.IsNullOrEmpty(authBody.ApiKey))
        {
            return AeroError.ValidationError(["Invalid auth body or missing ApiKey"]);
        }

        var query = @"
            query {
                me {
                    name
                    id
                    profilePicture
                    username
                }
            }";

        return await ExecuteGraphQLAsync<HashnodeMeResponse>(query, null, authBody.ApiKey, cancellationToken)
            .MapAsync<HashnodeMeResponse, AeroError, AuthTokenDetails>(response => new AuthTokenDetails
            {
                RefreshToken = "",
                ExpiresIn = (int)TimeSpan.FromDays(100).TotalSeconds,
                AccessToken = authBody.ApiKey,
                Id = response.Me.Id ?? "",
                Name = response.Me.Name ?? "",
                Picture = response.Me.ProfilePicture ?? string.Empty,
                Username = response.Me.Username ?? ""
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

        var firstPost = posts[0];
        var settings = firstPost.Settings ?? new Dictionary<string, object>();

        var title = GetSettingValue<string>(settings, "title") ?? "";
        var publication = GetSettingValue<string>(settings, "publication") ?? "";
        var canonical = GetSettingValue<string>(settings, "canonical");
        var tags = GetSettingValue<List<HashnodeTag>>(settings, "tags") ?? new List<HashnodeTag>();
        var subtitle = GetSettingValue<string>(settings, "subtitle");
        var mainImage = GetSettingValue<MediaContent>(settings, "main_image");

        var inputObj = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["publicationId"] = publication,
            ["contentMarkdown"] = firstPost.Message
        };

        if (!string.IsNullOrEmpty(canonical))
        {
            inputObj["originalArticleURL"] = canonical;
        }

        if (tags.Count > 0)
        {
            inputObj["tags"] = tags.Select(t => new { id = t.Value }).ToArray();
        }

        if (!string.IsNullOrEmpty(subtitle))
        {
            inputObj["subtitle"] = subtitle;
        }

        if (mainImage?.Path != null)
        {
            inputObj["coverImageOptions"] = new { coverImageURL = mainImage.Path };
        }

        var mutation = @"
            mutation PublishPost($input: PublishPostInput!) {
                publishPost(input: $input) {
                    post {
                        id
                        url
                    }
                }
            }";

        var variables = new { input = inputObj };

        return await ExecuteGraphQLAsync<HashnodePublishResponse>(mutation, variables, accessToken, cancellationToken)
            .MapAsync<HashnodePublishResponse, AeroError, PostResponse[]>(response => new[]
            {
                new PostResponse
                {
                    Id = firstPost.Id,
                    Status = "completed",
                    PostId = response.PublishPost.Post.Id ?? "",
                    ReleaseUrl = response.PublishPost.Post.Url ?? ""
                }
            });
    }

        /// <summary>
    /// GetPublicationsAsync method.
    /// </summary>
public async Task<Result<List<HashnodePublication>, AeroError>> GetPublicationsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var query = @"
            query {
                me {
                    publications(first: 50) {
                        edges {
                            node {
                                id
                                title
                            }
                        }
                    }
                }
            }";

        var result = await ExecuteGraphQLAsync<HashnodePublicationsResponse>(query, null, accessToken, cancellationToken);
        if (result is Result<HashnodePublicationsResponse, AeroError>.Failure failure)
        {
            return failure.Error;
        }

        var okValue = ((Result<HashnodePublicationsResponse, AeroError>.Ok)result).Value;

        return okValue.Me.Publications.Edges
            .Select(e => new HashnodePublication { Id = e.Node.Id ?? "", Name = e.Node.Title ?? "" })
            .ToList();
    }

    private async Task<Result<T, AeroError>> ExecuteGraphQLAsync<T>(
        string query,
        object? variables,
        string accessToken,
        CancellationToken cancellationToken)
        where T : class
    {
        var payload = new { query, variables };
        var request = CreateRequest(GraphQLEndpoint, HttpMethod.Post, payload);
        request.Headers.TryAddWithoutValidation("Authorization", accessToken);

        var result = await SendRequestAsync<GraphQLResponse<T>>(request, cancellationToken);
        
        return result.Map(r => r.Data);
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

    private class HashnodeAuthBody
    {
                /// <summary>
        /// Gets or sets the Api Key.
        /// </summary>
[JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = string.Empty;
    }

    private class GraphQLResponse<T>
    {
                /// <summary>
        /// Gets or sets the Data.
        /// </summary>
[JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }

    private class HashnodeMeResponse
    {
                /// <summary>
        /// Gets or sets the Me.
        /// </summary>
[JsonPropertyName("me")]
        public HashnodeMe Me { get; set; } = new();
    }

    private class HashnodeMe
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string? Id { get; set; }

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
        /// Gets or sets the Profile Picture.
        /// </summary>
[JsonPropertyName("profilePicture")]
        public string? ProfilePicture { get; set; }
    }

    private class HashnodePublishResponse
    {
                /// <summary>
        /// Gets or sets the Publish Post.
        /// </summary>
[JsonPropertyName("publishPost")]
        public HashnodePublishPost PublishPost { get; set; } = new();
    }

    private class HashnodePublishPost
    {
                /// <summary>
        /// Gets or sets the Post.
        /// </summary>
[JsonPropertyName("post")]
        public HashnodePost Post { get; set; } = new();
    }

    private class HashnodePost
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
    }

    private class HashnodePublicationsResponse
    {
                /// <summary>
        /// Gets or sets the Me.
        /// </summary>
[JsonPropertyName("me")]
        public HashnodeMePublications Me { get; set; } = new();
    }

    private class HashnodeMePublications
    {
                /// <summary>
        /// Gets or sets the Publications.
        /// </summary>
[JsonPropertyName("publications")]
        public HashnodePublications Publications { get; set; } = new();
    }

    private class HashnodePublications
    {
                /// <summary>
        /// Gets or sets the Edges.
        /// </summary>
[JsonPropertyName("edges")]
        public List<HashnodePublicationEdge> Edges { get; set; } = new();
    }

    private class HashnodePublicationEdge
    {
                /// <summary>
        /// Gets or sets the Node.
        /// </summary>
[JsonPropertyName("node")]
        public HashnodePublicationNode Node { get; set; } = new();
    }

    private class HashnodePublicationNode
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
    }

        /// <summary>
    /// Represents a class for HashnodeTag.
    /// </summary>
public class HashnodeTag
    {
                /// <summary>
        /// Gets or sets the Value.
        /// </summary>
public string Value { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Label.
        /// </summary>
public string Label { get; set; } = string.Empty;
    }

        /// <summary>
    /// Represents a class for HashnodePublication.
    /// </summary>
public class HashnodePublication
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
