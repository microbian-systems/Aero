using Microsoft.AspNetCore.Components;

namespace Aero.MerakiUI.Inputs;

/// <summary>
/// Represents a class for PasswordInput.
/// </summary>
public partial class PasswordInput : MerakiComponentBase
{
        /// <summary>
    /// Gets or sets the Placeholder.
    /// </summary>
[Parameter]
    public string? Placeholder { get; set; } = "Password";

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
    /// Gets or sets the Show Password.
    /// </summary>
protected bool ShowPassword { get; set; }

        /// <summary>
    /// Gets or sets the Input Type.
    /// </summary>
protected string InputType => ShowPassword ? "text" : "password";

        /// <summary>
    /// OnInput method.
    /// </summary>
protected async Task OnInput(ChangeEventArgs e)
    {
        Value = e.Value?.ToString();
        await ValueChanged.InvokeAsync(Value);
    }

        /// <summary>
    /// ToggleShowPassword method.
    /// </summary>
protected void ToggleShowPassword()
    {
        ShowPassword = !ShowPassword;
    }
}
