using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Sidebars;

/// <summary>
/// Represents a class for SimpleSidebar.
/// </summary>
public partial class SimpleSidebar : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Brand Name.
    /// </summary>
[Parameter]
    public string BrandName { get; set; } = "Brand";
}
