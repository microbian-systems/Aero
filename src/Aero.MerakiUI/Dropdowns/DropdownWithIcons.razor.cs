using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Dropdowns;

/// <summary>
/// Represents a class for DropdownWithIcons.
/// </summary>
public partial class DropdownWithIcons : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Trigger Text.
    /// </summary>
[Parameter]
    public string TriggerText { get; set; } = "Dropdown";
}
