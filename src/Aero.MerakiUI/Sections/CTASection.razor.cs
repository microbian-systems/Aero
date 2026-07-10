using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Sections;

/// <summary>
/// Represents a class for CTASection.
/// </summary>
public partial class CTASection : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Bring your Business to the next level.";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string? Description { get; set; }

        /// <summary>
    /// Gets or sets the Button Text.
    /// </summary>
[Parameter]
    public string ButtonText { get; set; } = "Sign Up";

        /// <summary>
    /// Gets or sets the Button Url.
    /// </summary>
[Parameter]
    public string ButtonUrl { get; set; } = "#";
}
