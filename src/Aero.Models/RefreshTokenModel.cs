namespace Aero.Models;

/// <summary>
/// Represents a class for RefreshTokenModel.
/// </summary>
public class RefreshTokenModel
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
[JsonPropertyName("id")]
    public string Id { get; set; }
        
        /// <summary>
    /// Gets or sets the Refresh Token.
    /// </summary>
[JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
}