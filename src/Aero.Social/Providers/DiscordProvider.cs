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
/// Provides integration with Discord for authenticating users and posting messages.
/// </summary>
/// <param name="httpClient">The HTTP client instance.</param>
/// <param name="configuration">The configuration instance.</param>
/// <param name="logger">The logger instance.</param>
public class DiscordProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<DiscordProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    private readonly IConfiguration _configuration = configuration;

    /// <inheritdoc/>
    public override string Identifier => "discord";

    /// <inheritdoc/>
    public override string Name => "Discord";

    /// <inheritdoc/>
    public override string[] Scopes => ["identify", "email", "guilds", "webhook.incoming"];

    /// <inheritdoc/>
    public override EditorType Editor => EditorType.Discord;

    /// <inheritdoc/>
    public override int MaxConcurrentJobs => 5;

    /// <inheritdoc/>
    public override int MaxLength(object? additionalSettings = null) => 2000;

    /// <inheritdoc/>
    public override Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(6);

        return Task.FromResult(GetClientId().Bind(clientId =>
            GetFrontendUrl().Map(frontendUrl =>
            {
                var url = $"https://discord.com/oauth2/authorize" +
                          $"?client_id={clientId}" +
                          $"&permissions=377957124096" +
                          $"&response_type=code" +
                          $"&redirect_uri={Uri.EscapeDataString($"{frontendUrl}/integrations/social/discord")}" +
                          $"&integration_type=0" +
                          $"&scope=bot+identify+guilds" +
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
        return await ExchangeCodeForTokenAsync(parameters.Code, cancellationToken)
            .BindAsync(async tokenResponse =>
            {
                var scopeCheck = CheckScopes(Scopes, tokenResponse.Scope);
                if (scopeCheck is Result<NoneType, AeroError>.Failure failure)
                {
                    return failure.Error;
                }

                return await GetApplicationInfoAsync(tokenResponse.AccessToken, cancellationToken)
                    .MapAsync(applicationInfo => new AuthTokenDetails
                    {
                        Id = tokenResponse.Guild?.Id ?? string.Empty,
                        Name = applicationInfo.Name,
                        AccessToken = tokenResponse.AccessToken,
                        RefreshToken = tokenResponse.RefreshToken,
                        ExpiresIn = tokenResponse.ExpiresIn,
                        Picture = $"https://cdn.discordapp.com/avatars/{applicationInfo.Bot.Id}/{applicationInfo.Bot.Avatar}.png",
                        Username = applicationInfo.Bot.Username
                    });
            });
    }

    /// <inheritdoc/>
    public override async Task<Result<AuthTokenDetails, AeroError>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        return await GetBasicCredentials().BindAsync(async credentials =>
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token")
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Basic {credentials}");

            return await SendRequestAsync<DiscordTokenResponse>(request, cancellationToken);
        }).BindAsync(async tokenResponse =>
        {
            return await GetApplicationInfoAsync(tokenResponse.AccessToken, cancellationToken)
                .MapAsync(applicationInfo => new AuthTokenDetails
                {
                    RefreshToken = tokenResponse.RefreshToken,
                    ExpiresIn = tokenResponse.ExpiresIn,
                    AccessToken = tokenResponse.AccessToken,
                    Id = string.Empty,
                    Name = applicationInfo.Name,
                    Picture = string.Empty,
                    Username = string.Empty
                });
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
        var channel = firstPost.Settings?.GetValueOrDefault("channel")?.ToString();
        
        if (string.IsNullOrEmpty(channel))
        {
            return AeroError.BadRequestError("Channel is required");
        }

        var form = new MultipartFormDataContent();
        
        var message = FormatMessage(firstPost.Message);
        var payload = new
        {
            content = message,
            attachments = firstPost.Media?.Select((m, index) => new
            {
                id = index,
                description = $"Picture {index}",
                filename = GetFileName(m.Path)
            }).ToList()
        };

        form.Add(new StringContent(JsonSerializer.Serialize(payload)), "payload_json");

        if (firstPost.Media != null)
        {
            foreach (var (media, i) in firstPost.Media.Select((m, index) => (m, index)))
            {
                var mediaResult = await ReadOrFetchAsync(media.Path, cancellationToken);
                if (mediaResult is Result<byte[], AeroError>.Ok(var bytes))
                {
                    form.Add(new ByteArrayContent(bytes), $"files[{i}]", GetFileName(media.Path));
                }
                else if (mediaResult is Result<byte[], AeroError>.Failure(var error))
                {
                    return error;
                }
            }
        }

        return await GetBotToken().BindAsync(async botToken =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://discord.com/api/channels/{channel}/messages")
            {
                Content = form
            };
            request.Headers.Add("Authorization", $"Bot {botToken}");

            return await SendRequestAsync<DiscordMessageResponse>(request, cancellationToken);
        }).MapAsync(messageResponse => new[]
        {
            new PostResponse
            {
                Id = firstPost.Id,
                PostId = messageResponse.Id,
                ReleaseUrl = $"https://discord.com/channels/{id}/{channel}/{messageResponse.Id}",
                Status = "success"
            }
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
        var channel = commentPost.Settings?.GetValueOrDefault("channel")?.ToString();
        
        if (string.IsNullOrEmpty(channel))
        {
            return AeroError.BadRequestError("Channel is required");
        }

        string threadChannel;

        if (string.IsNullOrEmpty(lastCommentId))
        {
            var threadResult = await CreateThreadAsync(channel, postId, cancellationToken);
            if (threadResult is Result<DiscordThreadResponse, AeroError>.Failure(var error))
            {
                return error;
            }
            threadChannel = ((Result<DiscordThreadResponse, AeroError>.Ok)threadResult).Value.Id;
        }
        else
        {
            threadChannel = channel;
        }

        var form = new MultipartFormDataContent();
        
        var message = FormatMessage(commentPost.Message);
        var payload = new
        {
            content = message,
            attachments = commentPost.Media?.Select((m, index) => new
            {
                id = index,
                description = $"Picture {index}",
                filename = GetFileName(m.Path)
            }).ToList()
        };

        form.Add(new StringContent(JsonSerializer.Serialize(payload)), "payload_json");

        if (commentPost.Media != null)
        {
            foreach (var (media, i) in commentPost.Media.Select((m, index) => (m, index)))
            {
                var mediaResult = await ReadOrFetchAsync(media.Path, cancellationToken);
                if (mediaResult is Result<byte[], AeroError>.Ok(var bytes))
                {
                    form.Add(new ByteArrayContent(bytes), $"files[{i}]", GetFileName(media.Path));
                }
                else if (mediaResult is Result<byte[], AeroError>.Failure(var error))
                {
                    return error;
                }
            }
        }

        return await GetBotToken().BindAsync(async botToken =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://discord.com/api/channels/{threadChannel}/messages")
            {
                Content = form
            };
            request.Headers.Add("Authorization", $"Bot {botToken}");

            return await SendRequestAsync<DiscordMessageResponse>(request, cancellationToken);
        }).MapAsync(messageResponse => (PostResponse[]?)new[]
        {
            new PostResponse
            {
                Id = commentPost.Id,
                PostId = messageResponse.Id,
                ReleaseUrl = $"https://discord.com/channels/{id}/{threadChannel}/{messageResponse.Id}",
                Status = "success"
            }
        });
    }

    /// <inheritdoc/>
    public override async Task<Result<object?, AeroError>> MentionAsync(
        string token,
        MentionQuery query,
        string id,
        Integration integration,
        CancellationToken cancellationToken = default)
    {
        return await GetRolesAsync(id, cancellationToken)
            .BindAsync(async roles =>
            {
                return await SearchMembersAsync(id, query.Query, cancellationToken)
                    .MapAsync(members =>
                    {
                        var results = new List<MentionResult>();

                        var specialMentions = new[]
                        {
                            new MentionResult { Id = "here", Label = "here", Image = string.Empty, DoNotCache = true },
                            new MentionResult { Id = "everyone", Label = "everyone", Image = string.Empty, DoNotCache = true }
                        }.Where(m => m.Label.Contains(query.Query, StringComparison.OrdinalIgnoreCase));

                        results.AddRange(specialMentions);
                        results.AddRange(roles
                            .Where(r => r.Name.Contains(query.Query, StringComparison.OrdinalIgnoreCase) &&
                                        r.Name != "@everyone" && r.Name != "@here")
                            .Select(r => new MentionResult
                            {
                                Id = $"&{r.Id}",
                                Label = r.Name.TrimStart('@'),
                                Image = string.Empty,
                                DoNotCache = true
                            }));
                        results.AddRange(members.Select(m => new MentionResult
                        {
                            Id = m.User.Id,
                            Label = m.User.GlobalName ?? m.User.Username,
                            Image = $"https://cdn.discordapp.com/avatars/{m.User.Id}/{m.User.Avatar}.png"
                        }));

                        return (object?)results;
                    });
            });
    }

    /// <inheritdoc/>
    public override string? MentionFormat(string idOrHandle, string name)
    {
        if (name == "@here" || name == "@everyone")
        {
            return name;
        }

        return $"[[[@{idOrHandle.TrimStart('@')}]]]";
    }

    private async Task<Result<DiscordTokenResponse, AeroError>> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken)
    {
        return await GetBasicCredentials().BindAsync(credentials =>
            GetFrontendUrl().BindAsync(async frontendUrl =>
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["grant_type"] = "authorization_code",
                    ["redirect_uri"] = $"{frontendUrl}/integrations/social/discord"
                });

                var request = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token")
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Basic {credentials}");

                return await SendRequestAsync<DiscordTokenResponse>(request, cancellationToken);
            }));
    }

    private async Task<Result<DiscordApplicationInfo, AeroError>> GetApplicationInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/oauth2/@me");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        return await SendRequestAsync<DiscordApplicationInfo>(request, cancellationToken);
    }

    private async Task<Result<DiscordThreadResponse, AeroError>> CreateThreadAsync(string channelId, string messageId, CancellationToken cancellationToken)
    {
        return await GetBotToken().BindAsync(async botToken =>
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { name = "Thread", auto_archive_duration = 1440 }),
                System.Text.Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://discord.com/api/channels/{channelId}/messages/{messageId}/threads")
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Bot {botToken}");

            return await SendRequestAsync<DiscordThreadResponse>(request, cancellationToken);
        });
    }

    private async Task<Result<List<DiscordRole>, AeroError>> GetRolesAsync(string guildId, CancellationToken cancellationToken)
    {
        return await GetBotToken().BindAsync(async botToken =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://discord.com/api/guilds/{guildId}/roles");
            request.Headers.Add("Authorization", $"Bot {botToken}");

            return await SendRequestAsync<List<DiscordRole>>(request, cancellationToken);
        });
    }

    private async Task<Result<List<DiscordMember>, AeroError>> SearchMembersAsync(string guildId, string query, CancellationToken cancellationToken)
    {
        return await GetBotToken().BindAsync(async botToken =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://discord.com/api/guilds/{guildId}/members/search?query={Uri.EscapeDataString(query)}");
            request.Headers.Add("Authorization", $"Bot {botToken}");

            return await SendRequestAsync<List<DiscordMember>>(request, cancellationToken);
        });
    }

    private static string FormatMessage(string message)
    {
        return System.Text.RegularExpressions.Regex.Replace(message, @"\[\[\[(@.*?)]]]", match =>
        {
            return $"<{match.Groups[1].Value}>";
        });
    }

    private static string GetFileName(string path)
    {
        return path.Split('/').Last();
    }

    private Result<string, AeroError> GetClientId() => configuration["DISCORD_CLIENT_ID"] is { } value ? value : AeroError.CreateError("DISCORD_CLIENT_ID not configured");
    private Result<string, AeroError> GetClientSecret() => configuration["DISCORD_CLIENT_SECRET"] is { } value ? value : AeroError.CreateError("DISCORD_CLIENT_SECRET not configured");
    private Result<string, AeroError> GetBotToken() => configuration["DISCORD_BOT_TOKEN_ID"] is { } value ? value : AeroError.CreateError("DISCORD_BOT_TOKEN_ID not configured");
    private Result<string, AeroError> GetFrontendUrl() => configuration["FRONTEND_URL"] is { } value ? value : AeroError.CreateError("FRONTEND_URL not configured");
    
    private Result<string, AeroError> GetBasicCredentials()
    {
        return GetClientId().Bind(clientId => 
            GetClientSecret().Map(clientSecret =>
            {
                var credentials = $"{clientId}:{clientSecret}";
                return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credentials));
            }));
    }

    //#region DTOs

    private class DiscordTokenResponse
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
        
                /// <summary>
        /// Gets or sets the Guild.
        /// </summary>
[JsonPropertyName("guild")]
        public DiscordGuild? Guild { get; set; }
    }

    private class DiscordGuild
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class DiscordApplicationInfo
    {
                /// <summary>
        /// Gets or sets the Name.
        /// </summary>
[JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
                /// <summary>
        /// Gets or sets the Bot.
        /// </summary>
[JsonPropertyName("bot")]
        public DiscordBot Bot { get; set; } = new();
    }

    private class DiscordBot
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
                /// <summary>
        /// Gets or sets the Username.
        /// </summary>
[JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        
                /// <summary>
        /// Gets or sets the Avatar.
        /// </summary>
[JsonPropertyName("avatar")]
        public string Avatar { get; set; } = string.Empty;
    }

    private class DiscordMessageResponse
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class DiscordThreadResponse
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    private class DiscordRole
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
    }

    private class DiscordMember
    {
                /// <summary>
        /// Gets or sets the User.
        /// </summary>
[JsonPropertyName("user")]
        public DiscordUser User { get; set; } = new();
    }

    private class DiscordUser
    {
                /// <summary>
        /// Gets or sets the Id.
        /// </summary>
[JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
                /// <summary>
        /// Gets or sets the Username.
        /// </summary>
[JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        
                /// <summary>
        /// Gets or sets the Global Name.
        /// </summary>
[JsonPropertyName("global_name")]
        public string? GlobalName { get; set; }
        
                /// <summary>
        /// Gets or sets the Avatar.
        /// </summary>
[JsonPropertyName("avatar")]
        public string Avatar { get; set; } = string.Empty;
    }

    //#endregion
}
