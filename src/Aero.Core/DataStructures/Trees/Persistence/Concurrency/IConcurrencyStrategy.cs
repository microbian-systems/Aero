using Aero.Core.DataStructures.Trees.Persistence.Wal;

namespace Aero.Core.DataStructures.Trees.Persistence.Concurrency;

/// <summary>
/// Defines an interface for IConcurrencyStrategy.
/// </summary>
public interface IConcurrencyStrategy : IAsyncDisposable
{
        /// <summary>
    /// BeginReadAsync method.
    /// </summary>
ValueTask<IReadSnapshot> BeginReadAsync(long transactionId, CancellationToken ct = default);
        /// <summary>
    /// BeginWriteAsync method.
    /// </summary>
ValueTask BeginWriteAsync(long transactionId, long pageId, CancellationToken ct = default);
        /// <summary>
    /// ValidateAsync method.
    /// </summary>
ValueTask ValidateAsync(ITransactionContext txn, CancellationToken ct = default);
        /// <summary>
    /// OnCommitAsync method.
    /// </summary>
ValueTask OnCommitAsync(long transactionId, Lsn commitLsn, CancellationToken ct = default);
        /// <summary>
    /// OnAbortAsync method.
    /// </summary>
ValueTask OnAbortAsync(long transactionId, CancellationToken ct = default);
        /// <summary>
    /// Gets or sets the Level.
    /// </summary>
IsolationLevel Level { get; }
}
