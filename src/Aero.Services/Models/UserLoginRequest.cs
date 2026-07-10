namespace Aero.Services.Models;

/// <summary>
/// Represents a record for UserLoginRequest.
/// </summary>
public record UserLoginRequest
{
        /// <summary>
    /// Gets or sets the Username.
    /// </summary>
[JsonPropertyName("username")]
    public string Username { get; set; }
        /// <summary>
    /// Gets or sets the Password.
    /// </summary>
[JsonPropertyName("password")]
    public string Password { get; set; }
}