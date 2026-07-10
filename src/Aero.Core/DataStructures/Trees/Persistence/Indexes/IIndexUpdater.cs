namespace Aero.Core.DataStructures.Trees.Persistence.Indexes;

/// <summary>
/// Defines an interface for IIndexUpdater.
/// </summary>
public interface IIndexUpdater<TDocument>
    where TDocument : class
{
        /// <summary>
    /// OnInsertAsync method.
    /// </summary>
ValueTask OnInsertAsync(Guid id, TDocument document, CancellationToken ct);
        /// <summary>
    /// OnUpdateAsync method.
    /// </summary>
ValueTask OnUpdateAsync(Guid id, TDocument oldDoc, TDocument newDoc, CancellationToken ct);
        /// <summary>
    /// OnDeleteAsync method.
    /// </summary>
ValueTask OnDeleteAsync(Guid id, TDocument document, CancellationToken ct);
}
