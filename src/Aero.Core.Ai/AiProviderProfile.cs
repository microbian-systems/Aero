namespace Aero.Core.Ai;

public sealed record AiProviderProfile(
    string Id,
    string DisplayName,
    AiProviderKind Provider,
    bool Enabled,
    string? Endpoint,
    string? Model,
    string? ProtectedApiKey,
    float Temperature,
    int MaxOutputTokens,
    int TimeoutSeconds,
    bool StreamResponses,
    bool SaveUsageTelemetry,
    bool SupportsContentEnhancement)
{
    public bool HasApiKey => !string.IsNullOrWhiteSpace(ProtectedApiKey);
}

public sealed record AiRuntimeSettings(
    string ProviderId,
    string DisplayName,
    bool Enabled,
    AiProviderKind Provider,
    string? Endpoint,
    string? Model,
    string? ApiKey,
    float Temperature,
    int MaxOutputTokens,
    int TimeoutSeconds,
    bool StreamResponses,
    bool SaveUsageTelemetry,
    bool SupportsContentEnhancement);
