namespace Aero.Web.Models;

/// <summary>
/// Represents a record for ApiRegistrationRequest.
/// </summary>
public record ApiRegistrationRequest
{
        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
public string Email { get; set; } = string.Empty;
}