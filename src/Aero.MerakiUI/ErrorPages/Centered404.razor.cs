using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.ErrorPages;

/// <summary>
/// Represents a class for Centered404.
/// </summary>
public partial class Centered404 : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Page not found";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string Description { get; set; } = "The page you are looking for doesn't exist. Here are some helpful links:";

        /// <summary>
    /// Gets or sets the Icon Content.
    /// </summary>
[Parameter]
    public RenderFragment? IconContent { get; set; }

        /// <summary>
    /// Gets or sets the Show Back Button.
    /// </summary>
[Parameter]
    public bool ShowBackButton { get; set; } = true;

        /// <summary>
    /// Gets or sets the Back Button Text.
    /// </summary>
[Parameter]
    public string BackButtonText { get; set; } = "Go back";

        /// <summary>
    /// Gets or sets the Show Home Button.
    /// </summary>
[Parameter]
    public bool ShowHomeButton { get; set; } = true;

        /// <summary>
    /// Gets or sets the Home Button Text.
    /// </summary>
[Parameter]
    public string HomeButtonText { get; set; } = "Take me home";

        /// <summary>
    /// Gets or sets the On Back Click.
    /// </summary>
[Parameter]
    public EventCallback OnBackClick { get; set; }

        /// <summary>
    /// Gets or sets the On Home Click.
    /// </summary>
[Parameter]
    public EventCallback OnHomeClick { get; set; }
}
