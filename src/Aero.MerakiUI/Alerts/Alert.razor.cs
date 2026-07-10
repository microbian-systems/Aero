using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Alerts;

/// <summary>
/// Represents a class for Alert.
/// </summary>
public partial class Alert : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Type.
    /// </summary>
[Parameter]
    public AlertType Type { get; set; } = AlertType.Info;

        /// <summary>
    /// Gets or sets the Title.
    /// </summary>
[Parameter]
    public string? Title { get; set; }

        /// <summary>
    /// Gets or sets the Type Color Class.
    /// </summary>
protected string TypeColorClass => Type switch
    {
        AlertType.Success => "bg-emerald-500",
        AlertType.Info => "bg-blue-500",
        AlertType.Warning => "bg-yellow-500",
        AlertType.Error => "bg-red-500",
        _ => "bg-blue-500"
    };

        /// <summary>
    /// Gets or sets the Title Color Class.
    /// </summary>
protected string TitleColorClass => Type switch
    {
        AlertType.Success => "text-emerald-500 dark:text-emerald-400",
        AlertType.Info => "text-blue-500 dark:text-blue-400",
        AlertType.Warning => "text-yellow-500 dark:text-yellow-400",
        AlertType.Error => "text-red-500 dark:text-red-400",
        _ => "text-blue-500 dark:text-blue-400"
    };
}
