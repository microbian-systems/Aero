using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Sections;

/// <summary>
/// Represents a class for FeatureItem.
/// </summary>
public partial class FeatureItem : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Feature Title";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string Description { get; set; } = "Feature description goes here.";

        /// <summary>
    /// Gets or sets the Icon Content.
    /// </summary>
[Parameter]
    public RenderFragment? IconContent { get; set; }
}
