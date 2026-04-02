using Marten;
using Aero.Cms.Core.Modules;

namespace Aero.Cms.Web.Core.Modules;

/// <summary>
/// Marten-backed implementation of <see cref="IModuleStateStore"/>.
/// </summary>
public sealed class ModuleStateStore : IModuleStateStore
{
    private readonly IDocumentSession _session;

    public ModuleStateStore(IDocumentSession session)
    {
        _session = session;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ModuleStateDocument>> GetAllAsync(CancellationToken ct = default)
    {
        return _session.Query<ModuleStateDocument>().ToListAsync(ct);
    }

    /// <inheritdoc/>
    public Task SaveAllAsync(IEnumerable<ModuleStateDocument> modules, CancellationToken ct = default)
    {
        foreach (var module in modules)
        {
            _session.Store(module);
        }
        return _session.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public Task<ModuleStateDocument?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var id = $"{ModuleStateDocument.ModuleIdPrefix}{name}";
        return _session.LoadAsync<ModuleStateDocument>(id, ct);
    }
}
