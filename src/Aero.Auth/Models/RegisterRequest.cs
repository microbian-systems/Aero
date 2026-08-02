using System.ComponentModel.DataAnnotations;

namespace Aero.Auth.Models;

/// <summary>
/// Represents a class for RegisterRequest.
/// </summary>
public class RegisterRequest
{
        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
[Required] 
    [EmailAddress] 
    public string? Email { get; set; }

        /// <summary>
    /// Gets or sets the Password.
    /// </summary>
[Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

        /// <summary>
    /// Gets or sets the Confirm Password.
    /// </summary>
[DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }

        /// <summary>
    /// Gets or sets the First Name.
    /// </summary>
[Required] 
    public string? FirstName { get; set; }

        /// <summary>
    /// Gets or sets the Last Name.
    /// </summary>
[Required] 
    public string? LastName { get; set; }
}