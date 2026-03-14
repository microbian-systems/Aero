using Aero.Core.Entities;

namespace Aero.Models.Entities;

/// <summary>
/// Represents a message sent via the Contact Us form.
/// </summary>
public class ContactMessage : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
