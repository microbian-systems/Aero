namespace Aero.Web.Models;

/// <summary>
/// Represents a class for AuthResponse.
/// </summary>
public class AuthResponse(string accessToken, string refreshToken, DateTimeOffset expiration)
{
        /// <summary>
    /// Gets or sets the access Token.
    /// </summary>
public string accessToken { get; set; } = accessToken;
        /// <summary>
    /// Gets or sets the refresh Token.
    /// </summary>
public string refreshToken { get; set; } = refreshToken;
        /// <summary>
    /// Gets or sets the Expiration.
    /// </summary>
public DateTimeOffset Expiration { get; set; } = expiration;
}