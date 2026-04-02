namespace Aero.Cms.Web.Core.Modules;

/// <summary>
/// Repository interface for loading and saving module state documents.
/// </summary>
public interface IModuleStateStore
{
    /// <summary>
    /// Gets all module state documents from the store.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of all module state documents.</returns>
    Task<IReadOnlyList<ModuleStateDocument>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Saves all provided module state documents to the store.
    /// </summary>
    /// <param name="modules">The modules to save.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveAllAsync(IEnumerable<ModuleStateDocument> modules, CancellationToken ct = default);

    /// <summary>
    /// Gets a module state document by its name.
    /// </summary>
    /// <param name="name">The module name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The module state document if found, otherwise null.</returns>
    Task<ModuleStateDocument?> GetByNameAsync(string name, CancellationToken ct = default);
}
