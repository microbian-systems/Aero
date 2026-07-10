using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Tooltips;

/// <summary>
/// Represents a class for Tooltip.
/// </summary>
public partial class Tooltip : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Text.
    /// </summary>
[Parameter]
    public string Text { get; set; } = "Tooltip text";

        /// <summary>
    /// Gets or sets the Position.
    /// </summary>
[Parameter]
    public TooltipPosition Position { get; set; } = TooltipPosition.Top;

    private string GetPositionClass() => Position switch
    {
        TooltipPosition.Top => "-top-14 left-1/2 -translate-x-1/2",
        TooltipPosition.Bottom => "top-full mt-2 left-1/2 -translate-x-1/2",
        TooltipPosition.Left => "right-full mr-2 top-1/2 -translate-y-1/2",
        TooltipPosition.Right => "left-full ml-2 top-1/2 -translate-y-1/2",
        _ => "-top-14 left-1/2 -translate-x-1/2"
    };
}

/// <summary>
/// Defines an enumeration for TooltipPosition.
/// </summary>
public enum TooltipPosition
{
    Top,
    Bottom,
    Left,
    Right
}
