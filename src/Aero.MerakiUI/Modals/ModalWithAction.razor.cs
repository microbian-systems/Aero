using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Modals;

/// <summary>
/// Represents a class for ModalWithAction.
/// </summary>
public partial class ModalWithAction : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Trigger Text.
    /// </summary>
[Parameter]
    public string TriggerText { get; set; } = "Open Modal";
}
