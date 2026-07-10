using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Footers;

/// <summary>
/// Represents a class for SimpleFooter.
/// </summary>
public partial class SimpleFooter : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Brand Name.
    /// </summary>
[Parameter]
    public string BrandName { get; set; } = "Brand";
}
