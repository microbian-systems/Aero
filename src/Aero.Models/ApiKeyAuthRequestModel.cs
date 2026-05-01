namespace Aero.Models;

/// <summary>
/// Request model for API key authentication.
/// </summary>
public record ApiKeyAuthRequestModel : IApiKeyAuthRequestModel
{
    /// <summary>
    /// Gets or sets the unique identifier (used as API key).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty; // todo - this was an old construct for api key auth requests - we should only pass the key

    /// <summary>
    /// Gets or sets the API key. Maps to Id.
    /// </summary>
    [JsonPropertyName("api_key")]
    public string ApiKey
    {
        get => Id;
        init => Id = value;
    }
}
