using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Breadcrumbs;

/// <summary>
/// Represents a class for BreadcrumbItem.
/// </summary>
public partial class BreadcrumbItem : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Text.
    /// </summary>
[Parameter]
    public string Text { get; set; } = "Home";

        /// <summary>
    /// Gets or sets the Url.
    /// </summary>
[Parameter]
    public string Url { get; set; } = "#";

        /// <summary>
    /// Gets or sets the Is Active.
    /// </summary>
[Parameter]
    public bool IsActive { get; set; } = false;

        /// <summary>
    /// Gets or sets the Show Separator.
    /// </summary>
[Parameter]
    public bool ShowSeparator { get; set; } = true;

        /// <summary>
    /// Gets or sets the Icon Content.
    /// </summary>
[Parameter]
    public RenderFragment? IconContent { get; set; }

        /// <summary>
    /// Gets or sets the Separator Content.
    /// </summary>
[Parameter]
    public RenderFragment? SeparatorContent { get; set; }
}
