namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Defines an interface for ITransactionContext.
/// </summary>
public interface ITransactionContext : IAsyncDisposable
{
        /// <summary>
    /// Gets or sets the Transaction Id.
    /// </summary>
long TransactionId { get; }
        /// <summary>
    /// TrackRead method.
    /// </summary>
void TrackRead(long pageId);
        /// <summary>
    /// TrackWrite method.
    /// </summary>
void TrackWrite(long pageId, ReadOnlyMemory<byte> beforeImage);
        /// <summary>
    /// CommitAsync method.
    /// </summary>
ValueTask CommitAsync(CancellationToken ct = default);
        /// <summary>
    /// RollbackAsync method.
    /// </summary>
ValueTask RollbackAsync(CancellationToken ct = default);
        /// <summary>
    /// Gets or sets the Is Committed.
    /// </summary>
bool IsCommitted { get; }
        /// <summary>
    /// Gets or sets the Is Aborted.
    /// </summary>
bool IsAborted { get; }
        /// <summary>
    /// Gets or sets the Dirty Pages.
    /// </summary>
IReadOnlyDictionary<long, ReadOnlyMemory<byte>> DirtyPages { get; }
        /// <summary>
    /// Gets or sets the Read Set.
    /// </summary>
IReadOnlyDictionary<long, uint> ReadSet { get; }
}
