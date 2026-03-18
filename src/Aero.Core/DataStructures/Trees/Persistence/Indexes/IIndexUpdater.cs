namespace Aero.DataStructures.Trees.Persistence.Indexes;

public interface IIndexUpdater<TDocument>
    where TDocument : class
{
    ValueTask OnInsertAsync(Guid id, TDocument document, CancellationToken ct);
    ValueTask OnUpdateAsync(Guid id, TDocument oldDoc, TDocument newDoc, CancellationToken ct);
    ValueTask OnDeleteAsync(Guid id, TDocument document, CancellationToken ct);
}
