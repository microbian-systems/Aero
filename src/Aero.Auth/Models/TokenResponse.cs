namespace Aero.Auth.Models;

/// <summary>
/// Represents a class for TokenResponse.
/// </summary>
public class TokenResponse
{
        /// <summary>
    /// Gets or sets the Access Token.
    /// </summary>
public string AccessToken { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Refresh Token.
    /// </summary>
public string RefreshToken { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Token Type.
    /// </summary>
public string TokenType { get; set; } = "Bearer";
        /// <summary>
    /// Gets or sets the Expires In.
    /// </summary>
public int ExpiresIn { get; set; }
        /// <summary>
    /// Gets or sets the Scopes.
    /// </summary>
public string[] Scopes { get; set; } = [];
}