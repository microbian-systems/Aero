using Microsoft.Extensions.Configuration;

namespace Aero.Core.Ai;

public static class DefaultAiProviderProfiles
{
    public const string OpenAiProviderId = "openai";
    public const string AnthropicProviderId = "anthropic";
    public const string GoogleProviderId = "google";
    public const string GroqProviderId = "groq";
    public const string DeepSeekProviderId = "deepseek";
    public const string MiniMaxProviderId = "minimax";
    public const string MistralProviderId = "mistral";
    public const string XAiProviderId = "xai";
    public const string ZaiProviderId = "zai";
    public const string PerplexityProviderId = "perplexity";
    public const string AlibabaProviderId = "alibaba";
    public const string OpenRouterProviderId = "openrouter";
    public const string LmStudioProviderId = "lm-studio";
    public const string OpenCodeProviderId = "opencode";

    public static IReadOnlyList<AiProviderProfile> Create(IConfiguration? configuration = null)
    {
        configuration ??= new ConfigurationBuilder().Build();

        return
        [
            CreateProfile(configuration, OpenAiProviderId, "OpenAi", "OpenAI", AiProviderKind.OpenAi, "gpt-4o-mini"),
            CreateProfile(configuration, AnthropicProviderId, "Anthropic", "Anthropic", AiProviderKind.Anthropic, "claude-3-5-sonnet-latest"),
            CreateProfile(configuration, GoogleProviderId, "Google", "Google Gemini", AiProviderKind.Google, "gemini-2.0-flash"),
            CreateProfile(configuration, GroqProviderId, "Groq", "Groq", AiProviderKind.Groq, "llama-3.3-70b-versatile"),
            CreateProfile(configuration, DeepSeekProviderId, "DeepSeek", "DeepSeek", AiProviderKind.DeepSeek, "deepseek-chat"),
            CreateProfile(configuration, MiniMaxProviderId, "MiniMax", "MiniMax", AiProviderKind.MiniMax, null),
            CreateProfile(configuration, MistralProviderId, "Mistral", "Mistral", AiProviderKind.Mistral, "mistral-large-latest"),
            CreateProfile(configuration, XAiProviderId, "XAi", "xAI", AiProviderKind.XAi, "grok-2-latest"),
            CreateProfile(configuration, ZaiProviderId, "Zai", "Z.ai", AiProviderKind.Zai, null),
            CreateProfile(configuration, PerplexityProviderId, "Perplexity", "Perplexity", AiProviderKind.Perplexity, "sonar"),
            CreateProfile(configuration, AlibabaProviderId, "Alibaba", "Alibaba Cloud", AiProviderKind.Alibaba, null),
            CreateProfile(configuration, OpenRouterProviderId, "OpenRouter", "OpenRouter", AiProviderKind.OpenRouter, "openai/gpt-4o-mini"),
            CreateProfile(
                configuration,
                LmStudioProviderId,
                "LmStudio",
                "LM Studio",
                AiProviderKind.LmStudio,
                Get(configuration, "Ai:Providers:LmStudio:Model", "local-model"),
                defaultEndpoint: Get(configuration, "Ai:Providers:LmStudio:Endpoint", "http://localhost:1234/v1"),
                supportsContentEnhancement: true),
            CreateProfile(
                configuration,
                OpenCodeProviderId,
                "OpenCode",
                "OpenCode",
                AiProviderKind.OpenCode,
                Get(configuration, "Ai:Providers:OpenCode:Model", "gpt-4o-mini"),
                defaultEndpoint: Get(configuration, "Ai:Providers:OpenCode:Endpoint", "http://localhost:3000/v1"),
                supportsContentEnhancement: true)
        ];
    }

    public static string GetDefaultProviderId(IConfiguration? configuration = null)
        => Get(configuration ?? new ConfigurationBuilder().Build(), "Ai:DefaultProviderId", OpenCodeProviderId)
            ?? OpenCodeProviderId;

    private static AiProviderProfile CreateProfile(
        IConfiguration configuration,
        string id,
        string configurationName,
        string displayName,
        AiProviderKind provider,
        string? defaultModel,
        string? defaultEndpoint = null,
        bool supportsContentEnhancement = true)
    {
        return new AiProviderProfile(
            id,
            Get(configuration, $"Ai:Providers:{configurationName}:DisplayName", displayName) ?? displayName,
            provider,
            GetBool(configuration, $"Ai:Providers:{configurationName}:Enabled", false),
            Get(configuration, $"Ai:Providers:{configurationName}:Endpoint", defaultEndpoint),
            Get(configuration, $"Ai:Providers:{configurationName}:Model", defaultModel),
            ProtectedApiKey: null,
            GetFloat(configuration, $"Ai:Providers:{configurationName}:Temperature", 0.3f),
            GetInt(configuration, $"Ai:Providers:{configurationName}:MaxOutputTokens", 128000),
            GetInt(configuration, $"Ai:Providers:{configurationName}:TimeoutSeconds", 60),
            GetBool(configuration, $"Ai:Providers:{configurationName}:StreamResponses", false),
            GetBool(configuration, $"Ai:Providers:{configurationName}:SaveUsageTelemetry", false),
            supportsContentEnhancement);
    }

    private static string? Get(IConfiguration configuration, string key, string? fallback)
        => string.IsNullOrWhiteSpace(configuration[key]) ? fallback : configuration[key];

    private static bool GetBool(IConfiguration configuration, string key, bool fallback)
        => bool.TryParse(configuration[key], out var value) ? value : fallback;

    private static int GetInt(IConfiguration configuration, string key, int fallback)
        => int.TryParse(configuration[key], out var value) ? value : fallback;

    private static float GetFloat(IConfiguration configuration, string key, float fallback)
        => float.TryParse(configuration[key], out var value) ? value : fallback;
}
