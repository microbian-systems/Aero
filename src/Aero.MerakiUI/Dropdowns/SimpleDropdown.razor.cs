using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Dropdowns;

/// <summary>
/// Represents a class for SimpleDropdown.
/// </summary>
public partial class SimpleDropdown : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Trigger Text.
    /// </summary>
[Parameter]
    public string TriggerText { get; set; } = "Dropdown";
}
