using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Sections;

/// <summary>
/// Represents a class for PricingSection.
/// </summary>
public partial class PricingSection : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Pricing";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string Description { get; set; } = "Choose the plan that's right for you.";
}
