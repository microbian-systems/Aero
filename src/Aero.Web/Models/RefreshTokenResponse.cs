namespace Aero.Web.Models;

/// <summary>
/// Represents a record for RefreshTokenResponse.
/// </summary>
public record RefreshTokenResponse
{
        /// <summary>
    /// Gets or sets the Access Token.
    /// </summary>
[JsonPropertyName("access_token")] 
    public string? AccessToken { get; set; }
    
        /// <summary>
    /// Gets or sets the Refresh Token.
    /// </summary>
[JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

        /// <summary>
    /// Gets or sets the Expiration.
    /// </summary>
[JsonPropertyName("Expiration")]
    public DateTimeOffset? Expiration { get; set; }
}