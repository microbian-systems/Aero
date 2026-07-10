using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Tables;

/// <summary>
/// Represents a class for SimpleTable.
/// </summary>
public partial class SimpleTable : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Headers.
    /// </summary>
[Parameter]
    public string[] Headers { get; set; } = Array.Empty<string>();
}
