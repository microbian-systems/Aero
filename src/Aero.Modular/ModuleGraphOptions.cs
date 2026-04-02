namespace Aero.Cms.Web.Core.Modules;

/// <summary>
/// Options for configuring module graph behavior.
/// </summary>
public sealed class ModuleGraphOptions
{
    /// <summary>
    /// Whether to throw on validation errors or just log warnings.
    /// </summary>
    public bool StrictMode { get; init; } = true;

    /// <summary>
    /// Whether to validate semantic version format for module versions.
    /// </summary>
    public bool ValidateSemanticVersions { get; init; } = false;
}
