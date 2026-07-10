using System.ComponentModel.DataAnnotations;

namespace Aero.Auth.Models.ViewModels;

/// <summary>
/// Represents a class for RegisterViewModel.
/// </summary>
public class RegisterViewModel
{
        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
[Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

        /// <summary>
    /// Gets or sets the Password.
    /// </summary>
[Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

        /// <summary>
    /// Gets or sets the Confirm Password.
    /// </summary>
[DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
    /// Gets or sets the Accept Terms.
    /// </summary>
[Display(Name = "I accept the Terms of Service")]
    public bool AcceptTerms { get; set; }
}
