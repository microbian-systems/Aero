using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Faq;

/// <summary>
/// Represents a class for FaqAccordion.
/// </summary>
public partial class FaqAccordion : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string Title { get; set; } = "FAQ's";
}
