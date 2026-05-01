namespace Aero.Modular;

/// <summary>
/// Assembly-level attribute emitted by the per-module <see cref="ModuleManifestGenerator"/>
/// to mark which type in the assembly implements <see cref="IModuleManifestProvider"/>.
/// The host <see cref="HostModuleCatalogGenerator"/> reads this attribute from
/// referenced assembly metadata to aggregate module descriptors without
/// executing the provider's IL at build time.
/// </summary>
/// <remarks>
/// Important limit: the host generator can read the provider type from this
/// attribute, but it cannot call <c>GetDescriptors()</c> at build time.
/// Cross-project graph validation (duplicate names, missing deps, cycles)
/// is therefore runtime-only in v1 unless module metadata is flattened into
/// source-generator-readable inputs (assembly attributes, additional files, etc.).
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ModuleManifestProviderAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="ModuleManifestProviderAttribute"/>.
    /// </summary>
    /// <param name="providerType">
    /// The <see cref="IModuleManifestProvider"/> implementation type in this assembly.
    /// </param>
    public ModuleManifestProviderAttribute(Type providerType)
    {
        ProviderType = providerType;
    }

    /// <summary>
    /// The <see cref="IModuleManifestProvider"/> implementation type.
    /// </summary>
    public Type ProviderType { get; }
}
