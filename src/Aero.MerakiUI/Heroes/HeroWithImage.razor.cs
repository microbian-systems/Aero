using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Heroes;

/// <summary>
/// Represents a class for HeroWithImage.
/// </summary>
public partial class HeroWithImage : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Build your next project";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string Description { get; set; } = "Lorem ipsum dolor sit amet.";

        /// <summary>
    /// Gets or sets the Image Url.
    /// </summary>
[Parameter]
    public string ImageUrl { get; set; } = "";
}
