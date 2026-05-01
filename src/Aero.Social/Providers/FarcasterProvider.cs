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

public class FarcasterProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<FarcasterProvider> logger)
    : SocialProviderBase(httpClient, logger)
{
    public override string Identifier => "wrapcast";
    public override string Name => "Farcaster";
    public override string[] Scopes => Array.Empty<string>();
    public override int MaxConcurrentJobs => 3;
    public override bool IsWeb3 => true;

    public override int MaxLength(object? additionalSettings = null) => 800;

    public override async Task<Result<GenerateAuthUrlResponse, AeroError>> GenerateAuthUrlAsync(
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        var state = MakeId(17);
        var clientId = GetNeynarClientId();

        return new GenerateAuthUrlResponse
        {
            Url = $"{clientId}||{state}",
            CodeVerifier = MakeId(10),
            State = state
        };
    }

    public override async Task<Result<AuthTokenDetails, AeroError>> AuthenticateAsync(
        AuthenticateParams parameters,
        ClientInformation? clientInformation = null,
        CancellationToken cancellationToken = default)
    {
        FarcasterAuthData? data;
        try
        {
            var dataBytes = Convert.FromBase64String(parameters.Code);
            var dataJson = Encoding.UTF8.GetString(dataBytes);
            data = JsonSerializer.Deserialize<FarcasterAuthData>(dataJson);
        }
        catch (Exception ex)
        {
            return AeroError.ValidationError([$"Invalid auth data: {ex.Message}"]);
        }

        if (data == null)
        {
            return AeroError.ValidationError(["Invalid auth data"]);
        }

        return new AuthTokenDetails
        {
            Id = data.Fid.ToString(),
            Name = data.DisplayName ?? "",
            AccessToken = data.SignerUuid ?? "",
            RefreshToken = "",
            ExpiresIn = (int)TimeSpan.FromDays(200).TotalSeconds,
            Picture = data.PfpUrl ?? string.Empty,
            Username = data.Username ?? ""
        };
    }

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

        var channels = GetSettingValue<List<FarcasterChannel>>(settings, "subreddit") ?? new List<FarcasterChannel>();
        if (channels.Count == 0)
            channels.Add(new FarcasterChannel());

        var results = new List<(string PostId, string ReleaseUrl)>();

        foreach (var channel in channels)
        {
            var payload = new Dictionary<string, object?>
            {
                ["signer_uuid"] = accessToken,
                ["text"] = firstPost.Message
            };

            if (firstPost.Media != null && firstPost.Media.Count > 0)
            {
                payload["embeds"] = firstPost.Media.Select(m => new { url = m.Path }).ToArray();
            }

            if (!string.IsNullOrEmpty(channel.Value?.Id))
            {
                payload["channel_id"] = channel.Value.Id;
            }

            var publishResult = await PublishCastAsync(payload, cancellationToken);
            if (publishResult is Result<FarcasterCastResponse, AeroError>.Ok ok)
            {
                results.Add((ok.Value.Hash, $"https://warpcast.com/{ok.Value.Username}/{ok.Value.Hash}"));
            }
            else if (publishResult is Result<FarcasterCastResponse, AeroError>.Failure failure)
            {
                return failure.Error;
            }
        }

        return new[]
        {
            new PostResponse
            {
                Id = firstPost.Id,
                PostId = string.Join(",", results.Select(r => r.PostId)),
                ReleaseUrl = string.Join(",", results.Select(r => r.ReleaseUrl)),
                Status = "published"
            }
        };
    }

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
        var parentIds = (lastCommentId ?? postId).Split(',');

        var results = new List<(string PostId, string ReleaseUrl)>();

        foreach (var parentHash in parentIds)
        {
            var payload = new Dictionary<string, object?>
            {
                ["signer_uuid"] = accessToken,
                ["text"] = commentPost.Message,
                ["parent"] = parentHash
            };

            if (commentPost.Media != null && commentPost.Media.Count > 0)
            {
                payload["embeds"] = commentPost.Media.Select(m => new { url = m.Path }).ToArray();
            }

            var publishResult = await PublishCastAsync(payload, cancellationToken);
            if (publishResult is Result<FarcasterCastResponse, AeroError>.Ok ok)
            {
                results.Add((ok.Value.Hash, $"https://warpcast.com/{ok.Value.Username}/{ok.Value.Hash}"));
            }
            else if (publishResult is Result<FarcasterCastResponse, AeroError>.Failure failure)
            {
                return failure.Error;
            }
        }

        return (PostResponse[]?)new[]
        {
            new PostResponse
            {
                Id = commentPost.Id,
                PostId = string.Join(",", results.Select(r => r.PostId)),
                ReleaseUrl = string.Join(",", results.Select(r => r.ReleaseUrl)),
                Status = "published"
            }
        };
    }

    public async Task<Result<List<FarcasterChannel>, AeroError>> SearchChannelsAsync(string query, CancellationToken cancellationToken = default)
    {
        var apiKey = GetNeynarApiKey();
        var url = $"https://api.neynar.com/v2/farcaster/channel/search?q={Uri.EscapeDataString(query)}&limit=10";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("x-api-key", apiKey);

        return await SendRequestAsync<FarcasterChannelSearchResponse>(request, cancellationToken)
            .MapAsync<FarcasterChannelSearchResponse, AeroError, List<FarcasterChannel>>(searchResult => 
                searchResult.Channels?.Select(c => new FarcasterChannel
                {
                    Title = c.Name ?? "",
                    Name = c.Name ?? "",
                    Id = c.Id ?? ""
                }).ToList() ?? new List<FarcasterChannel>());
    }

    private async Task<Result<FarcasterCastResponse, AeroError>> PublishCastAsync(Dictionary<string, object?> payload, CancellationToken cancellationToken)
    {
        var apiKey = GetNeynarApiKey();
        var request = CreateRequest("https://api.neynar.com/v2/farcaster/cast", HttpMethod.Post, payload);
        request.Headers.TryAddWithoutValidation("x-api-key", apiKey);

        return await SendRequestAsync<NeynarPublishCastResponse>(request, cancellationToken)
            .MapAsync<NeynarPublishCastResponse, AeroError, FarcasterCastResponse>(result => new FarcasterCastResponse
            {
                Hash = result.Cast?.Hash ?? "",
                Username = result.Cast?.Author?.Username ?? ""
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

    private string GetNeynarApiKey() => configuration["NEYNAR_SECRET_KEY"] ?? "00000000-000-0000-000-000000000000";
    private string GetNeynarClientId() => configuration["NEYNAR_CLIENT_ID"] ?? "";

    //#region DTOs

    private class FarcasterAuthData
    {
        [JsonPropertyName("fid")]
        public long? Fid { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("signer_uuid")]
        public string? SignerUuid { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("pfp_url")]
        public string? PfpUrl { get; set; }
    }

    private class NeynarPublishCastResponse
    {
        [JsonPropertyName("cast")]
        public NeynarCast? Cast { get; set; }
    }

    private class NeynarCast
    {
        [JsonPropertyName("hash")]
        public string? Hash { get; set; }

        [JsonPropertyName("author")]
        public NeynarAuthor? Author { get; set; }
    }

    private class NeynarAuthor
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }

    private class FarcasterCastResponse
    {
        public string Hash { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    private class FarcasterChannelSearchResponse
    {
        [JsonPropertyName("channels")]
        public List<FarcasterChannelData>? Channels { get; set; }
    }

    private class FarcasterChannelData
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class FarcasterChannel
    {
        public string Title { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public FarcasterChannelValue? Value { get; set; }
    }

    public class FarcasterChannelValue
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    //#endregion
}
