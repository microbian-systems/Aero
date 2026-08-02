using System.ComponentModel.DataAnnotations;

namespace Aero.Auth.Models;

/// <summary>
/// Represents a class for LoginRequest.
/// </summary>
public class LoginRequest
{
        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
[Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

        /// <summary>
    /// Gets or sets the Password.
    /// </summary>
[Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

        /// <summary>
    /// Gets or sets the Remember Me.
    /// </summary>
public bool RememberMe { get; set; } = false;
}