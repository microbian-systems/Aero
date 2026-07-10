using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Navbars;

/// <summary>
/// Represents a class for NavbarLink.
/// </summary>
public partial class NavbarLink : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Href.
    /// </summary>
[Parameter]
    public string? Href { get; set; } = "#";

        /// <summary>
    /// Gets or sets the Is Active.
    /// </summary>
[Parameter]
    public bool IsActive { get; set; }
}
