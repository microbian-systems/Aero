using System.ComponentModel.DataAnnotations;

namespace Aero.Auth.Models.ViewModels;

/// <summary>
/// Represents a class for ForgotPasswordViewModel.
/// </summary>
public class ForgotPasswordViewModel
{
        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
[Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
}
