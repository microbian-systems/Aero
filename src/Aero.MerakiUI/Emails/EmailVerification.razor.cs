using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Emails;

/// <summary>
/// Represents a class for EmailVerification.
/// </summary>
public partial class EmailVerification : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Logo Src.
    /// </summary>
[Parameter]
    public string LogoSrc { get; set; } = "https://merakiui.com/images/full-logo.svg";

        /// <summary>
    /// Gets or sets the Logo Url.
    /// </summary>
[Parameter]
    public string LogoUrl { get; set; } = "#";

        /// <summary>
    /// Gets or sets the User Name.
    /// </summary>
[Parameter]
    public string UserName { get; set; } = "Olivia";

        /// <summary>
    /// Gets or sets the Verification Message.
    /// </summary>
[Parameter]
    public string VerificationMessage { get; set; } = "This is your verification code:";

        /// <summary>
    /// Gets or sets the Verification Code.
    /// </summary>
[Parameter]
    public string VerificationCode { get; set; } = "6289";

        /// <summary>
    /// Gets or sets the Expiry Message.
    /// </summary>
[Parameter]
    public string ExpiryMessage { get; set; } = "This code will only be valid for the next 5 minutes. If the code does not work, you can use this login verification link:";

        /// <summary>
    /// Gets or sets the Show Verify Button.
    /// </summary>
[Parameter]
    public bool ShowVerifyButton { get; set; } = true;

        /// <summary>
    /// Gets or sets the Verify Button Text.
    /// </summary>
[Parameter]
    public string VerifyButtonText { get; set; } = "Verify email";

        /// <summary>
    /// Gets or sets the Team Name.
    /// </summary>
[Parameter]
    public string TeamName { get; set; } = "Meraki UI team";

        /// <summary>
    /// Gets or sets the Recipient Email.
    /// </summary>
[Parameter]
    public string RecipientEmail { get; set; } = "contact@merakiui.com";

        /// <summary>
    /// Gets or sets the Unsubscribe Url.
    /// </summary>
[Parameter]
    public string UnsubscribeUrl { get; set; } = "#";

        /// <summary>
    /// Gets or sets the Preferences Url.
    /// </summary>
[Parameter]
    public string PreferencesUrl { get; set; } = "#";

        /// <summary>
    /// Gets or sets the On Verify Click.
    /// </summary>
[Parameter]
    public EventCallback OnVerifyClick { get; set; }
}
