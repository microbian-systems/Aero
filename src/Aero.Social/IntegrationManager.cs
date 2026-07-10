using Aero.Social.Abstractions;
using Aero.Social.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Aero.Social;

/// <summary>
/// Represents a class for IntegrationManager.
/// </summary>
public class IntegrationManager
{
    private readonly Dictionary<string, ISocialProvider> _providers;
    private readonly IServiceProvider _serviceProvider;

        /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationManager"/> class.
    /// </summary>
public IntegrationManager(IServiceProvider serviceProvider, IEnumerable<ISocialProvider> providers)
    {
        _serviceProvider = serviceProvider;
        _providers = providers.ToDictionary(p => p.Identifier, p => p, StringComparer.OrdinalIgnoreCase);
    }

        /// <summary>
    /// GetSocialIntegration method.
    /// </summary>
public ISocialProvider GetSocialIntegration(string identifier)
    {
        if (_providers.TryGetValue(identifier, out var provider))
        {
            return provider;
        }

        throw new KeyNotFoundException($"Social provider '{identifier}' not found");
    }

        /// <summary>
    /// GetAllowedSocialIntegrations method.
    /// </summary>
public IEnumerable<string> GetAllowedSocialIntegrations()
    {
        return _providers.Keys;
    }

        /// <summary>
    /// GetAllIntegrationsAsync method.
    /// </summary>
public async Task<List<ProviderInfo>> GetAllIntegrationsAsync()
    {
        var result = new List<ProviderInfo>();

        foreach (var provider in _providers.Values)
        {
            var info = new ProviderInfo
            {
                Name = provider.Name,
                Identifier = provider.Identifier,
                Tooltip = provider.Tooltip,
                Editor = provider.Editor.ToString().ToLowerInvariant(),
                IsExternal = false,
                IsWeb3 = provider.IsWeb3,
                CustomFields = null
            };

            result.Add(info);
        }

        return result;
    }

        /// <summary>
    /// GetAllTools method.
    /// </summary>
public Dictionary<string, List<ToolInfo>> GetAllTools()
    {
        return _providers.Values.ToDictionary(
            p => p.Identifier,
            p => new List<ToolInfo>()
        );
    }

        /// <summary>
    /// GetAllRulesDescriptions method.
    /// </summary>
public Dictionary<string, string> GetAllRulesDescriptions()
    {
        return _providers.Values.ToDictionary(
            p => p.Identifier,
            p => string.Empty
        );
    }
}

/// <summary>
/// Represents a record for ProviderInfo.
/// </summary>
public record ProviderInfo
{
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public string Name { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Identifier.
    /// </summary>
public string Identifier { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Tooltip.
    /// </summary>
public string? Tooltip { get; init; }
        /// <summary>
    /// Gets or sets the Editor.
    /// </summary>
public string Editor { get; init; } = "normal";
        /// <summary>
    /// Gets or sets the Is External.
    /// </summary>
public bool IsExternal { get; init; }
        /// <summary>
    /// Gets or sets the Is Web3.
    /// </summary>
public bool IsWeb3 { get; init; }
        /// <summary>
    /// Gets or sets the Custom Fields.
    /// </summary>
public List<CustomField>? CustomFields { get; init; }
}

/// <summary>
/// Represents a record for CustomField.
/// </summary>
public record CustomField
{
        /// <summary>
    /// Gets or sets the Key.
    /// </summary>
public string Key { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Label.
    /// </summary>
public string Label { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Default Value.
    /// </summary>
public string? DefaultValue { get; init; }
        /// <summary>
    /// Gets or sets the Validation.
    /// </summary>
public string Validation { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Type.
    /// </summary>
public string Type { get; init; } = "text";
}

/// <summary>
/// Represents a record for ToolInfo.
/// </summary>
public record ToolInfo
{
        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
public string Description { get; init; } = string.Empty;
        /// <summary>
    /// Gets or sets the Data Schema.
    /// </summary>
public object DataSchema { get; init; } = new();
        /// <summary>
    /// Gets or sets the Method Name.
    /// </summary>
public string MethodName { get; init; } = string.Empty;
}

/// <summary>
/// Represents a class for SocialProviderExtensions.
/// </summary>
public static class SocialProviderExtensions
{
        /// <summary>
    /// AddSocialProviders method.
    /// </summary>
public static IServiceCollection AddSocialProviders(this IServiceCollection services)
    {
        services.AddScoped<IntegrationManager>();
        
        services.AddHttpClient<DiscordProvider>();
        services.AddHttpClient<SlackProvider>();
        services.AddHttpClient<TelegramProvider>();
        services.AddHttpClient<MediumProvider>();
        services.AddHttpClient<LinkedInProvider>();
        services.AddHttpClient<FacebookProvider>();
        services.AddHttpClient<XProvider>();
        services.AddHttpClient<RedditProvider>();
        services.AddHttpClient<InstagramProvider>();
        services.AddHttpClient<TikTokProvider>();
        services.AddHttpClient<YouTubeProvider>();
        services.AddHttpClient<PinterestProvider>();
        services.AddHttpClient<ThreadsProvider>();
        services.AddHttpClient<BlueskyProvider>();
        services.AddHttpClient<MastodonProvider>();
        services.AddHttpClient<LemmyProvider>();
        services.AddHttpClient<FarcasterProvider>();
        services.AddHttpClient<NostrProvider>();
        services.AddHttpClient<VkProvider>();
        services.AddHttpClient<DevToProvider>();
        services.AddHttpClient<HashnodeProvider>();
        services.AddHttpClient<WordPressProvider>();
        services.AddHttpClient<ListmonkProvider>();
        services.AddHttpClient<DribbbleProvider>();
        services.AddHttpClient<TwitchProvider>();
        services.AddHttpClient<KickProvider>();
        services.AddHttpClient<GmbProvider>();
        services.AddHttpClient<LinkedInPageProvider>();
        services.AddHttpClient<InstagramStandaloneProvider>();

        services.AddTransient<ISocialProvider, DiscordProvider>();
        services.AddTransient<ISocialProvider, SlackProvider>();
        services.AddTransient<ISocialProvider, TelegramProvider>();
        services.AddTransient<ISocialProvider, MediumProvider>();
        services.AddTransient<ISocialProvider, LinkedInProvider>();
        services.AddTransient<ISocialProvider, FacebookProvider>();
        services.AddTransient<ISocialProvider, XProvider>();
        services.AddTransient<ISocialProvider, RedditProvider>();
        services.AddTransient<ISocialProvider, InstagramProvider>();
        services.AddTransient<ISocialProvider, TikTokProvider>();
        services.AddTransient<ISocialProvider, YouTubeProvider>();
        services.AddTransient<ISocialProvider, PinterestProvider>();
        services.AddTransient<ISocialProvider, ThreadsProvider>();
        services.AddTransient<ISocialProvider, BlueskyProvider>();
        services.AddTransient<ISocialProvider, MastodonProvider>();
        services.AddTransient<ISocialProvider, LemmyProvider>();
        services.AddTransient<ISocialProvider, FarcasterProvider>();
        services.AddTransient<ISocialProvider, NostrProvider>();
        services.AddTransient<ISocialProvider, VkProvider>();
        services.AddTransient<ISocialProvider, DevToProvider>();
        services.AddTransient<ISocialProvider, HashnodeProvider>();
        services.AddTransient<ISocialProvider, WordPressProvider>();
        services.AddTransient<ISocialProvider, ListmonkProvider>();
        services.AddTransient<ISocialProvider, DribbbleProvider>();
        services.AddTransient<ISocialProvider, TwitchProvider>();
        services.AddTransient<ISocialProvider, KickProvider>();
        services.AddTransient<ISocialProvider, GmbProvider>();
        services.AddTransient<ISocialProvider, LinkedInPageProvider>();
        services.AddTransient<ISocialProvider, InstagramStandaloneProvider>();

        return services;
    }
}
