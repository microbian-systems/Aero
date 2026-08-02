using System.ComponentModel.DataAnnotations;

namespace Aero.Auth.Models.ViewModels;

/// <summary>
/// Represents a class for PasskeyViewModel.
/// </summary>
public class PasskeyViewModel
{
        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
public string? Email { get; set; }
        /// <summary>
    /// Gets or sets the Display Name.
    /// </summary>
public string? DisplayName { get; set; }
        /// <summary>
    /// Gets or sets the Is Registration.
    /// </summary>
public bool IsRegistration { get; set; }
}

/// <summary>
/// Represents a class for PasskeyRegistrationViewModel.
/// </summary>
public class PasskeyRegistrationViewModel
{
        /// <summary>
    /// Gets or sets the Passkey Name.
    /// </summary>
[Required(ErrorMessage = "Passkey Name is required")]
    [Display(Name = "Passkey Name")]
    public string PasskeyName { get; set; } = string.Empty;
    
        /// <summary>
    /// Gets or sets the Device Name.
    /// </summary>
[Display(Name = "Device Name")]
    public string? DeviceName { get; set; }
}
