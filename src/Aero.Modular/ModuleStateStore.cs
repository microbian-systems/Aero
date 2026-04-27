using Marten;

namespace Aero.Modular;

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
    public Task<IReadOnlyList<ModuleDocument>> GetAllAsync(CancellationToken ct = default)
    {
        return _session.Query<ModuleDocument>().ToListAsync(ct);
    }

    /// <inheritdoc/>
    public Task SaveAllAsync(IEnumerable<ModuleDocument> modules, CancellationToken ct = default)
    {
        foreach (var module in modules)
        {
            _session.Store(module);
        }
        return _session.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public Task<ModuleDocument?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var id = $"{ModuleDocument.ModuleIdPrefix}{name}";
        return _session.LoadAsync<ModuleDocument>(id, ct);
    }
}
