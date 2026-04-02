namespace Aero.Cms.Web.Core.Modules;

/// <summary>
/// Options for configuring module discovery behavior.
/// </summary>
public sealed class ModuleDiscoveryOptions
{
    /// <summary>
    /// Paths to additional directories to scan for module assemblies.
    /// </summary>
    public IReadOnlyList<string> AdditionalScanPaths { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Assembly name patterns to exclude from scanning (e.g., "System.*", "Microsoft.*").
    /// </summary>
    public IReadOnlyList<string> ExcludedAssemblyPatterns { get; init; } = new[]
    {
        "System.*",
        "Microsoft.*",
        "netstandard",
        "mscorlib",
        "NuGet.*",
        "Serilog.*",
        "Scrutor*",
        "EasyScrutor*"
    };

    /// <summary>
    /// Whether to validate semantic version format for module versions.
    /// </summary>
    public bool ValidateSemanticVersions { get; init; } = false;

    /// <summary>
    /// Whether to include modules marked as DisabledInProduction in production environments.
    /// </summary>
    public bool IncludeDisabledInProduction { get; init; } = false;

    /// <summary>
    /// Whether to scan assemblies in the application base directory.
    /// </summary>
    public bool ScanApplicationDependencies { get; init; } = true;

    /// <summary>
    /// Type filter for additional constraints on module types during discovery.
    /// Return true to include the type, false to exclude it.
    /// </summary>
    public Func<Type, bool>? TypeFilter { get; init; }
}
