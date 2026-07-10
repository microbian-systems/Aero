using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Forms;

/// <summary>
/// Represents a class for ContactForm.
/// </summary>
public partial class ContactForm : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "Contact Us";

        /// <summary>
    /// Gets or sets the Description.
    /// </summary>
[Parameter]
    public string Description { get; set; } = "Fill out the form below to get in touch.";
}
