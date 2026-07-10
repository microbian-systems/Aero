using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.ErrorPages;

/// <summary>
/// Represents a class for Illustration404.
/// </summary>
public partial class Illustration404 : MerakiComponentBase
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
    public string Title { get; set; } = "Page not found";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string Description { get; set; } = "Sorry, the page you are looking for doesn't exist. Here are some helpful links:";

        /// <summary>
    /// Gets or sets the Illustration Url.
    /// </summary>
[Parameter]
    public string IllustrationUrl { get; set; } = "/images/components/illustration.svg";

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
