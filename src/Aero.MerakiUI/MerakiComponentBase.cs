using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI;

/// <summary>
/// Represents a class for MerakiComponentBase.
/// </summary>
public abstract class MerakiComponentBase : ComponentBase
{
        /// <summary>
    /// Gets or sets the Class.
    /// </summary>
[Parameter]
    public string? Class { get; set; }

        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
[Parameter]
    public string? Id { get; set; }

        /// <summary>
    /// Gets or sets the Child Content.
    /// </summary>
[Parameter]
    public RenderFragment? ChildContent { get; set; }

        /// <summary>
    /// Gets or sets the Additional Attributes.
    /// </summary>
[Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
}
