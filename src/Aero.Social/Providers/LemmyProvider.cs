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
/// Represents a class for LemmyProvider.
/// </summary>
public class LemmyProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<LemmyProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    private readonly IConfiguration _configuration = configuration;

        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public override string Identifier => "lemmy";
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public override string Name => "Lemmy";
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public override string[] Scopes => Array.Empty<string>();
        /// <summary>
    /// Gets or sets the Max Concurrent Jobs.
    /// </summary>
public override int MaxConcurrentJobs => 3;

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
        var bodyBytes = Convert.FromBase64String(parameters.Code);
        var bodyJson = Encoding.UTF8.GetString(bodyBytes);
        var authBody = JsonSerializer.Deserialize<LemmyAuthBody>(bodyJson);
        if (authBody == null)
        {
            return AeroError.BadRequestError("Invalid auth body");
        }

        var loginUrl = $"{authBody.Service}/api/v3/user/login";

        var payload = new
        {
            username_or_email = authBody.Identifier,
            password = authBody.Password
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(loginUrl, content, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return AeroError.BadRequestError("Invalid credentials");
        }

        response.EnsureSuccessStatusCode();

        var loginResult = await DeserializeAsync<LemmyLoginResponse>(response);

        try
        {
            var userUrl = $"{authBody.Service}/api/v3/user?username={authBody.Identifier}";

            var userRequest = new HttpRequestMessage(HttpMethod.Get, userUrl);
            userRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {loginResult.Jwt}");

            var userResponse = await client.SendAsync(userRequest, cancellationToken);
            userResponse.EnsureSuccessStatusCode();

            var userResult = await DeserializeAsync<LemmyUserResponse>(userResponse);

            return new AuthTokenDetails
            {
                RefreshToken = loginResult.Jwt!,
                ExpiresIn = (int)TimeSpan.FromDays(100).TotalSeconds,
                AccessToken = loginResult.Jwt!,
                Id = userResult.PersonView.Person.Id.ToString(),
                Name = userResult.PersonView.Person.DisplayName
                       ?? userResult.PersonView.Person.Name
                       ?? "",
                Picture = userResult.PersonView.Person.Avatar ?? string.Empty,
                Username = authBody.Identifier
            };
        }
        catch (Exception)
        {
            return AeroError.BadRequestError("Invalid credentials");
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
        if (posts.Count == 0)
            return Array.Empty<PostResponse>();

        var authBody = GetAuthBody(integration);
        var jwt = await GetJwtAsync(authBody, cancellationToken);
        var firstPost = posts[0];

        var settings = firstPost.Settings ?? new Dictionary<string, object>();
        var subreddits = GetSettingValue<List<LemmySubreddit>>(settings, "subreddit") ?? new List<LemmySubreddit>();

        var valueArray = new List<PostResponse>();

        foreach (var lemmy in subreddits)
        {
            var payload = new Dictionary<string, object?>
            {
                ["community_id"] = lemmy.Value.Id,
                ["name"] = lemmy.Value.Title,
                ["body"] = firstPost.Message,
                ["nsfw"] = false
            };

            if (!string.IsNullOrEmpty(lemmy.Value.Url))
            {
                var url = lemmy.Value.Url;
                if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    url = $"https://{url}";
                }
                payload["url"] = url;
            }

            if (firstPost.Media != null && firstPost.Media.Count > 0)
            {
                payload["custom_thumbnail"] = firstPost.Media[0].Path;
            }

            var postUrl = $"{authBody.Service}/api/v3/post";
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, postUrl) { Content = content };
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {jwt}");

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var postResult = await DeserializeAsync<LemmyPostResponse>(response);

            valueArray.Add(new PostResponse
            {
                PostId = postResult.PostView.Post.Id.ToString(),
                ReleaseUrl = $"{authBody.Service}/post/{postResult.PostView.Post.Id}",
                Id = firstPost.Id,
                Status = "published"
            });
        }

        return new[]
        {
            new PostResponse
            {
                Id = firstPost.Id,
                PostId = string.Join(",", valueArray.Select(p => p.PostId)),
                ReleaseUrl = string.Join(",", valueArray.Select(p => p.ReleaseUrl)),
                Status = "published"
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

        var authBody = GetAuthBody(integration);
        var jwt = await GetJwtAsync(authBody, cancellationToken);
        var commentPost = posts[0];

        var postIds = postId.Split(',');
        var valueArray = new List<PostResponse>();

        foreach (var singlePostId in postIds)
        {
            var payload = new
            {
                post_id = int.Parse(singlePostId),
                content = commentPost.Message
            };

            var commentUrl = $"{authBody.Service}/api/v3/comment";
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, commentUrl) { Content = content };
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {jwt}");

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var commentResult = await DeserializeAsync<LemmyCommentResponse>(response);

            valueArray.Add(new PostResponse
            {
                PostId = commentResult.CommentView.Comment.Id.ToString(),
                ReleaseUrl = $"{authBody.Service}/comment/{commentResult.CommentView.Comment.Id}",
                Id = commentPost.Id,
                Status = "published"
            });
        }

        return new[]
        {
            new PostResponse
            {
                Id = commentPost.Id,
                PostId = string.Join(",", valueArray.Select(p => p.PostId)),
                ReleaseUrl = string.Join(",", valueArray.Select(p => p.ReleaseUrl)),
                Status = "published"
            }
        };
    }

        /// <summary>
    /// SearchCommunitiesAsync method.
    /// </summary>
public async Task<List<LemmyCommunity>> SearchCommunitiesAsync(
        Integration integration,
        string query,
        CancellationToken cancellationToken = default)
    {
        var authBody = GetAuthBody(integration);
        var jwt = await GetJwtAsync(authBody, cancellationToken);

        var url = $"{authBody.Service}/api/v3/search?type_=Communities&sort=Active&q={Uri.EscapeDataString(query)}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {jwt}");

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var searchResult = await DeserializeAsync<LemmySearchResponse>(response);

        return searchResult.Communities?.Select(c => new LemmyCommunity
        {
            Title = c.Community.Title,
            Name = c.Community.Name,
            Id = c.Community.Id
        }).ToList() ?? new List<LemmyCommunity>();
    }

    private async Task<string> GetJwtAsync(LemmyAuthBody authBody, CancellationToken cancellationToken)
    {
        var loginUrl = $"{authBody.Service}/api/v3/user/login";

        var payload = new
        {
            username_or_email = authBody.Identifier,
            password = authBody.Password
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(loginUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var loginResult = await DeserializeAsync<LemmyLoginResponse>(response);
        return loginResult.Jwt!;
    }

    private static LemmyAuthBody GetAuthBody(Integration integration)
    {
        if (string.IsNullOrEmpty(integration.CustomInstanceDetails)) return null;

        var jsonBytes = Convert.FromBase64String(integration.CustomInstanceDetails);
        var json = Encoding.UTF8.GetString(jsonBytes);
        return JsonSerializer.Deserialize<LemmyAuthBody>(json);
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

    private class LemmyAuthBody
    {
                /// <summary>
        /// Gets or sets the Service.
        /// </summary>
[JsonPropertyName("service")]
        public string Service { get; set; } = "https://lemmy.world";

                /// <summary>
        /// Gets or sets the Identifier.
        /// </summary>
[JsonPropertyName("identifier")]
        public string Identifier { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Password.
        /// </summary>
[JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    private class LemmyLoginResponse
    {
                /// <summary>
        /// Gets or sets the Jwt.
        /// </summary>
[JsonPropertyName("jwt")]
        public string? Jwt { get; set; }
    }

    private class LemmyUserResponse
    {
                /// <summary>
        /// Gets or sets the Person View.
        /// </summary>
[JsonPropertyName("person_view")]
        public LemmyPersonView PersonView { get; set; } = new();
    }

    private class LemmyPersonView
    {
                /// <summary>
        /// Gets or sets the Person.
        /// </summary>
[JsonPropertyName("person")]
        public LemmyPerson Person { get; set; } = new();
    }

    private class LemmyPerson
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
        /// Gets or sets the Display Name.
        /// </summary>
[JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

                /// <summary>
        /// Gets or sets the Avatar.
        /// </summary>
[JsonPropertyName("avatar")]
        public string? Avatar { get; set; }
    }

    private class LemmyPostResponse
    {
                /// <summary>
        /// Gets or sets the Post View.
        /// </summary>
[JsonPropertyName("post_view")]
        public LemmyPostView PostView { get; set; } = new();
    }

    private class LemmyPostView
    {
                /// <summary>
        /// Gets or sets the Post.
        /// </summary>
[JsonPropertyName("post")]
        public LemmyPost Post { get; set; } = new();
    }

    private class LemmyPost
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
    }

    private class LemmyCommentResponse
    {
                /// <summary>
        /// Gets or sets the Comment View.
        /// </summary>
[JsonPropertyName("comment_view")]
        public LemmyCommentView CommentView { get; set; } = new();
    }

    private class LemmyCommentView
    {
                /// <summary>
        /// Gets or sets the Comment.
        /// </summary>
[JsonPropertyName("comment")]
        public LemmyComment Comment { get; set; } = new();
    }

    private class LemmyComment
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public int Id { get; set; }
    }

    private class LemmySearchResponse
    {
                /// <summary>
        /// Gets or sets the Communities.
        /// </summary>
[JsonPropertyName("communities")]
        public List<LemmyCommunityView>? Communities { get; set; }
    }

    private class LemmyCommunityView
    {
                /// <summary>
        /// Gets or sets the Community.
        /// </summary>
[JsonPropertyName("community")]
        public LemmyCommunityDetail Community { get; set; } = new();
    }

    private class LemmyCommunityDetail
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
        public string Name { get; set; } = string.Empty;

                /// <summary>
        /// Gets or sets the Title.
        /// </summary>
[JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }

        /// <summary>
    /// Represents a class for LemmyCommunity.
    /// </summary>
public class LemmyCommunity
    {
                /// <summary>
        /// Gets or sets the Title.
        /// </summary>
public string Title { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
public string Name { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
public int Id { get; set; }
    }

        /// <summary>
    /// Represents a class for LemmySubreddit.
    /// </summary>
public class LemmySubreddit
    {
                /// <summary>
        /// Gets or sets the Value.
        /// </summary>
public LemmySubredditValue Value { get; set; } = new();
    }

        /// <summary>
    /// Represents a class for LemmySubredditValue.
    /// </summary>
public class LemmySubredditValue
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
public int Id { get; set; }
                /// <summary>
        /// Gets or sets the Title.
        /// </summary>
public string Title { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Url.
        /// </summary>
public string? Url { get; set; }
    }

    //#endregion
}
