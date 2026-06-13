using Aero.Core;
using Aero.Core.Railway;
using LlmTornado;
using LlmTornado.Chat.Models;
using LlmTornado.Code;
using LlmTornado.Microsoft.Extensions.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Aero.Core.Ai;

public sealed class TornadoAiChatClientFactory(
    ILogger<TornadoAiChatClientFactory> logger,
    TornadoProviderClient tornadoClient) : IAiChatClientFactory
{
    private static bool _tornadoConfigured;

    public Task<Result<IChatClient>> CreateAsync(
        AiRuntimeSettings settings,
        CancellationToken cancellationToken = default)
    {
        ConfigureTornadoOnce();

        if (!settings.Enabled)
        {
            return Task.FromResult<Result<IChatClient>>(AeroError.ConfigurationError("AI provider is disabled."));
        }

        if (!settings.SupportsContentEnhancement)
        {
            return Task.FromResult<Result<IChatClient>>(AeroError.ConfigurationError("Selected AI provider cannot enhance content."));
        }

        if (string.IsNullOrWhiteSpace(settings.Model))
        {
            return Task.FromResult<Result<IChatClient>>(AeroError.ConfigurationError("AI model is not configured."));
        }

        try
        {
            var api = CreateTornadoApi(settings);
            IChatClient client = api.AsChatClient(new ChatModel(settings.Model));
            return Task.FromResult<Result<IChatClient>>(new Result<IChatClient>.Ok(client));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create AI chat client for provider {ProviderId}.", settings.ProviderId);
            return Task.FromResult<Result<IChatClient>>(
                AeroError.ConfigurationError($"AI provider could not be initialized: {ex.Message}"));
        }
    }

    private void ConfigureTornadoOnce()
    {
        if (_tornadoConfigured) return;
        _tornadoConfigured = true;

        // Wire Tornado to our DI-managed typed HttpClient — no Polly retry handler attached.
        TornadoConfig.CreateClient = _ => tornadoClient.HttpClient;
        logger.LogInformation("LlmTornado HttpClient configured via DI (no automatic retry).");
    }

    private static TornadoApi CreateTornadoApi(AiRuntimeSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            // Use 3-arg constructor with LLmProviders.OpenAi so Tornado constructs the
            // standard OpenAI-compatible URL: {baseUri}/v1/chat/completions.
            // Strip trailing /v1 to avoid double-pathing (e.g. /zen/v1/v1/chat/completions).
            var baseUri = settings.Endpoint.TrimEnd('/');
            if (baseUri.EndsWith("/v1"))
            {
                baseUri = baseUri[..^3];
            }

            return new TornadoApi(new Uri(baseUri, UriKind.Absolute), settings.ApiKey, LLmProviders.OpenAi);
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException("AI API key is not configured.");
        }

        return new TornadoApi([new ProviderAuthentication(ToTornadoProvider(settings.Provider), settings.ApiKey)]);
    }

    private static LLmProviders ToTornadoProvider(AiProviderKind provider)
        => provider switch
        {
            AiProviderKind.OpenAi => LLmProviders.OpenAi,
            AiProviderKind.Anthropic => LLmProviders.Anthropic,
            AiProviderKind.Google => LLmProviders.Google,
            AiProviderKind.Groq => LLmProviders.Groq,
            AiProviderKind.DeepSeek => LLmProviders.DeepSeek,
            AiProviderKind.MiniMax => LLmProviders.MiniMax,
            AiProviderKind.Mistral => LLmProviders.Mistral,
            AiProviderKind.XAi => LLmProviders.XAi,
            AiProviderKind.Zai => LLmProviders.Zai,
            AiProviderKind.Perplexity => LLmProviders.Perplexity,
            AiProviderKind.Alibaba => LLmProviders.Alibaba,
            AiProviderKind.OpenRouter => LLmProviders.OpenRouter,
            _ => throw new NotSupportedException($"AI provider '{provider}' cannot be created with LLMTornado.")
        };
}
