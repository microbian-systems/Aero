namespace Aero.Models;

/// <summary>
/// Represents a record for RefreshTokenRequest.
/// </summary>
public record RefreshTokenRequest
{
        /// <summary>
    /// Gets or sets the Access Token.
    /// </summary>
[JsonPropertyName("token")]
    public string AccessToken { get; init; }

        /// <summary>
    /// Gets or sets the Refresh Token.
    /// </summary>
[JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }
}