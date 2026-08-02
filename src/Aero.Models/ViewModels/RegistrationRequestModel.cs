using Microsoft.AspNetCore.Identity;

namespace Aero.Models.ViewModels;

/// <summary>
/// Represents a record for RegistrationRequestModel.
/// </summary>
public record RegistrationRequestModel 
{
        /// <summary>
    /// Gets or sets the Username.
    /// </summary>
[JsonPropertyName("username")]
    public string Username { get; set; }
        
        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
[JsonPropertyName("email")]
    public string Email { get; set; }
        
        /// <summary>
    /// Gets or sets the Firstname.
    /// </summary>
[JsonPropertyName("firstname")]
    public string Firstname { get; set; }
        
        /// <summary>
    /// Gets or sets the Lastname.
    /// </summary>
[JsonPropertyName("lastname")]
    public string Lastname { get; set; }
        
        /// <summary>
    /// Gets or sets the Password.
    /// </summary>
[JsonPropertyName("password")]
    public string Password { get; set; }
        
        /// <summary>
    /// Gets or sets the Confirmed Password.
    /// </summary>
[JsonPropertyName("confirmed_password")]
    public string ConfirmedPassword { get; set; }
        
        /// <summary>
    /// Gets or sets the Birthday.
    /// </summary>
[PersonalData]
    [JsonPropertyName("birthday")]
    public DateTime? Birthday { get; set; }
        
        /// <summary>
    /// Gets or sets the Mobile Number.
    /// </summary>
[PersonalData]
    [JsonPropertyName("mobile_number")]
    public string MobileNumber { get; set; }
        
        /// <summary>
    /// Gets or sets the Postal Code.
    /// </summary>
[JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }
        
        /// <summary>
    /// Gets or sets the Country.
    /// </summary>
[JsonPropertyName("country")]
    public string Country { get; set; }
        
        /// <summary>
    /// Gets or sets the Agreed To Tos.
    /// </summary>
[JsonPropertyName("agreed_tos")]
    public bool AgreedToTos { get; set; }

        /// <summary>
    /// Gets or sets the Address.
    /// </summary>
[JsonPropertyName("address")]
    public string Address { get; set; }
}