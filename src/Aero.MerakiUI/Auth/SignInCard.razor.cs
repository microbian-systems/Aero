using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Auth;

/// <summary>
/// Represents a class for SignInCard.
/// </summary>
public partial class SignInCard : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Logo Src.
    /// </summary>
[Parameter]
    public string LogoSrc { get; set; } = "https://merakiui.com/images/logo.svg";

        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Welcome Back";

        /// <summary>
    /// Gets or sets the Subtitle.
    /// </summary>
[Parameter]
    public string Subtitle { get; set; } = "Login or create account";

        /// <summary>
    /// Gets or sets the Email Placeholder.
    /// </summary>
[Parameter]
    public string EmailPlaceholder { get; set; } = "Email Address";

        /// <summary>
    /// Gets or sets the Password Placeholder.
    /// </summary>
[Parameter]
    public string PasswordPlaceholder { get; set; } = "Password";

        /// <summary>
    /// Gets or sets the Button Text.
    /// </summary>
[Parameter]
    public string ButtonText { get; set; } = "Sign In";

        /// <summary>
    /// Gets or sets the Show Forgot Password.
    /// </summary>
[Parameter]
    public bool ShowForgotPassword { get; set; } = true;

        /// <summary>
    /// Gets or sets the Forgot Password Text.
    /// </summary>
[Parameter]
    public string ForgotPasswordText { get; set; } = "Forget Password?";

        /// <summary>
    /// Gets or sets the Forgot Password Url.
    /// </summary>
[Parameter]
    public string ForgotPasswordUrl { get; set; } = "#";

        /// <summary>
    /// Gets or sets the Show Footer.
    /// </summary>
[Parameter]
    public bool ShowFooter { get; set; } = true;

        /// <summary>
    /// Gets or sets the Footer Text.
    /// </summary>
[Parameter]
    public string FooterText { get; set; } = "Don't have an account?";

        /// <summary>
    /// Gets or sets the Footer Link Text.
    /// </summary>
[Parameter]
    public string FooterLinkText { get; set; } = "Register";

        /// <summary>
    /// Gets or sets the Footer Link Url.
    /// </summary>
[Parameter]
    public string FooterLinkUrl { get; set; } = "#";

        /// <summary>
    /// Gets or sets the Email.
    /// </summary>
[Parameter]
    public string Email { get; set; } = "";

        /// <summary>
    /// Gets or sets the Password.
    /// </summary>
[Parameter]
    public string Password { get; set; } = "";

        /// <summary>
    /// Gets or sets the On Submit.
    /// </summary>
[Parameter]
    public EventCallback OnSubmit { get; set; }
}
