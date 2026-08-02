namespace Aero.Core.Ai;

/// <summary>
/// Represents a record for AiProviderProfile.
/// </summary>
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
        /// <summary>
    /// Gets or sets the Has Api Key.
    /// </summary>
public bool HasApiKey => !string.IsNullOrWhiteSpace(ProtectedApiKey);
}

/// <summary>
/// Represents a record for AiRuntimeSettings.
/// </summary>
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
