using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Faq;

/// <summary>
/// Represents a class for FaqItem.
/// </summary>
public partial class FaqItem : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Question.
    /// </summary>
[Parameter]
    public string Question { get; set; } = "Question";

        /// <summary>
    /// Gets or sets the Answer.
    /// </summary>
[Parameter]
    public string Answer { get; set; } = "Answer";

        /// <summary>
    /// Gets or sets the Is Initially Open.
    /// </summary>
[Parameter]
    public bool IsInitiallyOpen { get; set; } = false;
}
