using Aero.Core.Entities;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a message sent via the Contact Us form.
/// </summary>
public class ContactMessage : Entity
{
        /// <summary>
    /// Gets or sets the Name.
    /// </summary>
public string Name { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
public string Email { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Message.
    /// </summary>
public string Message { get; set; } = string.Empty;
}
