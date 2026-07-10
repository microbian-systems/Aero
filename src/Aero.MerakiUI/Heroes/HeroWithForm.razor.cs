using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Heroes;

/// <summary>
/// Represents a class for HeroWithForm.
/// </summary>
public partial class HeroWithForm : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Title";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string Description { get; set; } = "";

        /// <summary>
    /// Gets or sets the Input Placeholder.
    /// </summary>
[Parameter]
    public string InputPlaceholder { get; set; } = "Email Address";

        /// <summary>
    /// Gets or sets the Button Text.
    /// </summary>
[Parameter]
    public string ButtonText { get; set; } = "Subscribe";
}
