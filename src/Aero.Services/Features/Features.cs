using Aero.Core.Entities;

namespace Aero.Services.Features;

/// <summary>
/// Represents a class for Features.
/// </summary>
public class Features : Entity
{
        /// <summary>
    /// Gets or sets the Application.
    /// </summary>
public string Application { get; set; }
        /// <summary>
    /// Gets or sets the Version.
    /// </summary>
public string Version { get; set; }
        /// <summary>
    /// Gets or sets the Feature.
    /// </summary>
public string Feature { get; set; }
        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
public string Description { get; set; }
        /// <summary>
    /// Gets or sets the Toggled.
    /// </summary>
public bool Toggled { get; set; }
        /// <summary>
    /// Gets or sets the Value.
    /// </summary>
public string Value { get; set; }
}