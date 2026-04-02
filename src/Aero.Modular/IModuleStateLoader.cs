namespace Aero.Cms.Web.Core.Modules;

using Aero.Cms.Core.Modules;

/// <summary>
/// Service responsible for loading module state from the database.
/// Provides a way to restore persisted module metadata without re-discovering via reflection.
/// </summary>
public interface IModuleStateLoader
{
    /// <summary>
    /// Checks whether any module state has been stored in the database.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if stored modules exist, false otherwise.</returns>
    Task<bool> HasStoredModulesAsync(CancellationToken ct = default);

    /// <summary>
    /// Loads all module state documents from the database.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of module state documents, or empty list if none exist.</returns>
    Task<IReadOnlyList<ModuleStateDocument>> LoadModuleStatesAsync(CancellationToken ct = default);

    /// <summary>
    /// Loads module state from the database and merges it with reflection-discovered descriptors.
    /// For each discovered module, if corresponding DB state exists, the DB state properties
    /// (Disabled, Order, Description, Category, Tags) take precedence.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of merged module descriptors.</returns>
    Task<IReadOnlyList<ModuleDescriptor>> LoadMergedModulesAsync(CancellationToken ct = default);
}
