namespace Aero.Cms.Core.Modules;

/// <summary>
/// Normalized metadata for a discovered module.
/// </summary>
[Obsolete("This class is deprecated. Use AeroModuleBase instead.", false)]
public sealed class ModuleDescriptor
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Author { get; init; }
    public required Type ModuleType { get; init; }
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    public required string AssemblyName { get; init; }
    public string? PhysicalPath { get; init; }
    public bool IsUiModule { get; init; }

    /// <summary>
    /// The load order priority for the module. Lower values load first.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Categories this module belongs to (e.g., "Security", "Content", "Infrastructure").
    /// </summary>
    public IReadOnlyList<string> Category { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Tags associated with the module for discovery purposes.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Whether this module should be disabled in production environments.
    /// </summary>
    public bool DisabledInProduction { get; init; }

    /// <summary>
    /// Whether this module has been disabled by the user.
    /// When true, the module will not be loaded regardless of other settings.
    /// </summary>
    public bool Disabled { get; init; }
}
