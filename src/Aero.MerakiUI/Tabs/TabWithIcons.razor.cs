using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Tabs;

/// <summary>
/// Represents a class for TabWithIcons.
/// </summary>
public partial class TabWithIcons : MerakiComponentBase
{
        /// <summary>
    /// Represents a class for TabItem.
    /// </summary>
public class TabItem
    {
                /// <summary>
        /// Gets or sets the Title.
        /// </summary>
public string Title { get; set; } = "";
                /// <summary>
        /// Gets or sets the Icon.
        /// </summary>
public string Icon { get; set; } = "";
    }

        /// <summary>
    /// Gets or sets the Items.
    /// </summary>
[Parameter]
    public List<TabItem> Items { get; set; } = new();
}
