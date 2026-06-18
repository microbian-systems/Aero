namespace Aero.Modular;

/// <summary>
/// Normalized metadata for a discovered module.
/// Remains the startup-time DTO for module identity and graph construction.
/// Marker-interface flags enable reflection-free specialized interface
/// registration from source-generated descriptors.
/// </summary>
public sealed class ModuleDescriptor
{
    /// <summary>Module display name.</summary>
    public required string Name { get; init; }

    /// <summary>Module version string.</summary>
    public required string Version { get; init; }

    /// <summary>Module author name.</summary>
    public required string Author { get; init; }

    /// <summary>The concrete <see cref="IAeroModule"/> implementation type.</summary>
    public required Type ModuleType { get; init; }

    /// <summary>Module dependencies (names of required modules).</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();

    /// <summary>The assembly containing the module class.</summary>
    public required string AssemblyName { get; init; }

    /// <summary>Optional physical path (for plugin-style modules).</summary>
    public string? PhysicalPath { get; init; }

    /// <summary>The load order priority for the module. Lower values load first.</summary>
    public int Order { get; init; }

    /// <summary>Categories this module belongs to.</summary>
    public IReadOnlyList<string> Category { get; init; } = Array.Empty<string>();

    /// <summary>Tags associated with the module.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Whether this module should be disabled in production environments.</summary>
    public bool DisabledInProduction { get; init; }

    /// <summary>
    /// Whether this module has been disabled by the user.
    /// When true, the module will not be loaded regardless of other settings.
    /// </summary>
    public bool Disabled { get; init; }

    /// <summary>Human-readable description of the module.</summary>
    public string? Description { get; init; }

    // ---- Marker interface flags (source-generated) ----

    /// <summary>Module implements <see cref="IUiModule"/>.</summary>
    public bool IsUiModule { get; init; }

    /// <summary>Module implements <see cref="IApiModule"/>.</summary>
    public bool IsApiModule { get; init; }

    /// <summary>Module implements <see cref="IBackgroundModule"/>.</summary>
    public bool IsBackgroundModule { get; init; }

    /// <summary>Module implements <see cref="IThemeModule"/>.</summary>
    public bool IsThemeModule { get; init; }

    /// <summary>Module implements <see cref="IAdminModule"/>.</summary>
    public bool IsAdminModule { get; init; }

    /// <summary>Module implements <see cref="IFilterModule"/>.</summary>
    public bool IsFilterModule { get; init; }

    /// <summary>Module implements <see cref="IContentDefinitionModule"/>.</summary>
    public bool IsContentDefinitionModule { get; init; }

    /// <summary>Module type implements <c>Marten.IConfigureMarten</c>.</summary>
    public bool IsMartenConfigurator { get; init; }

    /// <summary>Module type implements <c>Marten.IAsyncConfigureMarten</c>.</summary>
    public bool IsAsyncMartenConfigurator { get; init; }
}
