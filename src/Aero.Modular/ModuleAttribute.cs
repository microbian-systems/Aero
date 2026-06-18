namespace Aero.Modular;

/// <summary>
/// Compile-time metadata attribute for source-generated module discovery.
/// Applied to <see cref="IAeroModule"/> implementations to provide the
/// <see cref="ModuleManifestGenerator"/> with static module metadata
/// without instantiating the module at build time or startup.
/// </summary>
/// <remarks>
/// All attribute values must be compile-time constants.
/// This attribute does not replace <see cref="IAeroModule"/>;
/// it provides enough static data to build a manifest without
/// calling <c>Activator.CreateInstance</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ModuleAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="ModuleAttribute"/>.
    /// </summary>
    /// <param name="name">The module name (must be a compile-time constant).</param>
    /// <param name="version">Optional module version string.</param>
    /// <param name="author">Optional module author string.</param>
    public ModuleAttribute(string name, string? version = null, string? author = null)
    {
        Name = name;
        Version = version;
        Author = author;
    }

    /// <summary>Module display name.</summary>
    public string Name { get; }

    /// <summary>Module version string (e.g., "0.0.5-alpha").</summary>
    public string? Version { get; }

    /// <summary>Module author name.</summary>
    public string? Author { get; }

    /// <summary>Load order priority. Lower values load first.</summary>
    public short Order { get; init; }

    /// <summary>Dependencies — module names required by this module.</summary>
    public string[]? Dependencies { get; init; }

    /// <summary>Categories this module belongs to.</summary>
    public string[]? Category { get; init; }

    /// <summary>Tags associated with the module.</summary>
    public string[]? Tags { get; init; }

    /// <summary>Whether this module should be disabled in production environments.</summary>
    public bool DisabledInProduction { get; init; }

    /// <summary>Human-readable description of the module.</summary>
    public string? Description { get; init; }
}
