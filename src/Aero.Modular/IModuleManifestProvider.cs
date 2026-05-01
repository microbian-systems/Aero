namespace Aero.Modular;

/// <summary>
/// Provides source-generated module descriptor metadata for the host aggregator.
/// Each module project that declares an <see cref="IAeroModule"/> with
/// <see cref="ModuleAttribute"/> emits a generated implementation of this
/// interface into its own assembly, along with an assembly-level
/// <see cref="ModuleManifestProviderAttribute"/> that the host generator
/// reads for aggregation.
/// </summary>
/// <remarks>
/// Instance-based contract chosen over static abstract interface members
/// for simpler testability and more natural consumption from the host
/// aggregator, which emits direct <c>new Provider()</c> calls.
/// </remarks>
public interface IModuleManifestProvider
{
    /// <summary>
    /// Gets the list of module descriptors discovered in this assembly.
    /// </summary>
    IReadOnlyList<ModuleDescriptor> GetDescriptors();
}
