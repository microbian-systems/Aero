using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.ErrorPages;

/// <summary>
/// Represents a class for Simple404.
/// </summary>
public partial class Simple404 : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Error Code.
    /// </summary>
[Parameter]
    public string ErrorCode { get; set; } = "404 error";

        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "We can’t find that page";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string Description { get; set; } = "Sorry, the page you are looking for doesn't exist or has been moved.";

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
