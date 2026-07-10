using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Cookies;

/// <summary>
/// Represents a class for CookieBanner.
/// </summary>
public partial class CookieBanner : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Message.
    /// </summary>
[Parameter]
    public string Message { get; set; } = "We use cookies to ensure that we give you the best experience on our website.";

        /// <summary>
    /// Gets or sets the Policy Url.
    /// </summary>
[Parameter]
    public string PolicyUrl { get; set; } = "#";

        /// <summary>
    /// Gets or sets the Policy Link Text.
    /// </summary>
[Parameter]
    public string PolicyLinkText { get; set; } = "Read cookies policies";

        /// <summary>
    /// Gets or sets the Settings Button Text.
    /// </summary>
[Parameter]
    public string SettingsButtonText { get; set; } = "Cookie Setting";

        /// <summary>
    /// Gets or sets the Accept Button Text.
    /// </summary>
[Parameter]
    public string AcceptButtonText { get; set; } = "Accept All Cookies";

        /// <summary>
    /// Gets or sets the Icon Content.
    /// </summary>
[Parameter]
    public RenderFragment? IconContent { get; set; }

        /// <summary>
    /// Gets or sets the On Settings Click.
    /// </summary>
[Parameter]
    public EventCallback OnSettingsClick { get; set; }

        /// <summary>
    /// Gets or sets the On Accept Click.
    /// </summary>
[Parameter]
    public EventCallback OnAcceptClick { get; set; }
}
