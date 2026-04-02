namespace Aero.Cms.Web.Core.Modules;

using Aero.Cms.Core.Modules;

/// <summary>
/// Service responsible for discovering modules from assemblies.
/// </summary>
public interface IModuleDiscoveryService
{
    /// <summary>
    /// Discovers all modules from the application's assemblies.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of module descriptors representing discovered modules.</returns>
    Task<IReadOnlyList<ModuleDescriptor>> DiscoverAsync(CancellationToken ct = default);
}
