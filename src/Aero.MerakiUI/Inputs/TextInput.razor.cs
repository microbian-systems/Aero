using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Inputs;

/// <summary>
/// Represents a class for TextInput.
/// </summary>
public partial class TextInput : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Label.
    /// </summary>
[Parameter]
    public string? Label { get; set; }

        /// <summary>
    /// Gets or sets the Placeholder.
    /// </summary>
[Parameter]
    public string? Placeholder { get; set; }

        /// <summary>
    /// Gets or sets the Value.
    /// </summary>
[Parameter]
    public string? Value { get; set; }

        /// <summary>
    /// Gets or sets the Value Changed.
    /// </summary>
[Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

        /// <summary>
    /// OnInput method.
    /// </summary>
protected async Task OnInput(ChangeEventArgs e)
    {
        Value = e.Value?.ToString();
        await ValueChanged.InvokeAsync(Value);
    }
}
