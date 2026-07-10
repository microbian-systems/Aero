using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Tabs;

/// <summary>
/// Represents a class for SimpleTabs.
/// </summary>
public partial class SimpleTabs : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Tabs.
    /// </summary>
[Parameter]
    public string[] Tabs { get; set; } = Array.Empty<string>();
}
