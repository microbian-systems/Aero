using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Modals;

/// <summary>
/// Represents a class for SimpleModal.
/// </summary>
public partial class SimpleModal : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Trigger Text.
    /// </summary>
[Parameter]
    public string TriggerText { get; set; } = "Open Modal";
}
