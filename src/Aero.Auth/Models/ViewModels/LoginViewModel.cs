using System.ComponentModel.DataAnnotations;

namespace Aero.Auth.Models.ViewModels;

/// <summary>
/// Represents a class for LoginViewModel.
/// </summary>
public class LoginViewModel
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
    /// Gets or sets the Remember Me.
    /// </summary>
[Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
    
        /// <summary>
    /// Gets or sets the Return Url.
    /// </summary>
public string? ReturnUrl { get; set; }
}
