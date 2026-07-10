using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Auth;

/// <summary>
/// Represents a class for SignUpCard.
/// </summary>
public partial class SignUpCard : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Logo Src.
    /// </summary>
[Parameter]
    public string LogoSrc { get; set; } = "https://merakiui.com/images/logo.svg";

        /// <summary>
    /// Gets or sets the Sign In Url.
    /// </summary>
[Parameter]
    public string SignInUrl { get; set; } = "#";

        /// <summary>
    /// Gets or sets the Button Text.
    /// </summary>
[Parameter]
    public string ButtonText { get; set; } = "Sign Up";

        /// <summary>
    /// Gets or sets the Show File Upload.
    /// </summary>
[Parameter]
    public bool ShowFileUpload { get; set; } = true;

        /// <summary>
    /// Gets or sets the File Upload Text.
    /// </summary>
[Parameter]
    public string FileUploadText { get; set; } = "Profile Photo";

        /// <summary>
    /// Gets or sets the Already Have Account Text.
    /// </summary>
[Parameter]
    public string AlreadyHaveAccountText { get; set; } = "Already have an account?";

        /// <summary>
    /// Gets or sets the Username.
    /// </summary>
[Parameter]
    public string Username { get; set; } = "";

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
    /// Gets or sets the Confirm Password.
    /// </summary>
[Parameter]
    public string ConfirmPassword { get; set; } = "";

        /// <summary>
    /// Gets or sets the On Submit.
    /// </summary>
[Parameter]
    public EventCallback OnSubmit { get; set; }
}
