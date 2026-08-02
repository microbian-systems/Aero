using Aero.Core.Entities;

namespace Aero.Models;

/// <summary>
/// Represents a class for AeroUserSettings.
/// </summary>
public class AeroUserSettings : Entity
{
        /// <summary>
    /// Gets or sets the User Id.
    /// </summary>
public string UserId { get; set; } // foreign key
        /// <summary>
    /// Gets or sets the Stuff.
    /// </summary>
public object Stuff { get; set; }
}