using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Buttons;

/// <summary>
/// Represents a class for IconButton.
/// </summary>
public partial class IconButton : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Icon Content.
    /// </summary>
[Parameter]
    public RenderFragment? IconContent { get; set; }
}
