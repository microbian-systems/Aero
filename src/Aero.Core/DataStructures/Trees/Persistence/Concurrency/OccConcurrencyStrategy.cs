using System.Collections.Concurrent;
using Aero.Core.DataStructures.Trees.Persistence.Wal;

namespace Aero.Core.DataStructures.Trees.Persistence.Concurrency;

/// <summary>
/// Represents a class for OccConcurrencyStrategy.
/// </summary>
public sealed class OccConcurrencyStrategy : IConcurrencyStrategy
{
    private readonly ConcurrentDictionary<long, uint> _committedVersions = new();
    private readonly object _validateLock = new();

        /// <summary>
    /// Gets or sets the Level.
    /// </summary>
public IsolationLevel Level => IsolationLevel.OptimisticOCC;

        /// <summary>
    /// BeginReadAsync method.
    /// </summary>
public ValueTask<IReadSnapshot> BeginReadAsync(long txnId, CancellationToken ct = default)
    {
        return ValueTask.FromResult<IReadSnapshot>(new UnboundedSnapshot(txnId));
    }

        /// <summary>
    /// BeginWriteAsync method.
    /// </summary>
public ValueTask BeginWriteAsync(long txnId, long pageId, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

        /// <summary>
    /// ValidateAsync method.
    /// </summary>
public ValueTask ValidateAsync(ITransactionContext txn, CancellationToken ct = default)
    {
        lock (_validateLock)
        {
            foreach (var (pageId, versionAtRead) in txn.ReadSet)
            {
                if (!_committedVersions.TryGetValue(pageId, out var currentVersion))
                    continue;

                if (currentVersion != versionAtRead)
                    throw new ConflictException(txn.TransactionId, pageId);
            }

            foreach (var pageId in txn.DirtyPages.Keys)
            {
                _committedVersions.AddOrUpdate(
                    pageId,
                    addValue: 1,
                    updateValueFactory: (_, v) => v + 1);
            }
        }

        return ValueTask.CompletedTask;
    }

        /// <summary>
    /// OnCommitAsync method.
    /// </summary>
public ValueTask OnCommitAsync(long txnId, Lsn commitLsn, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

        /// <summary>
    /// OnAbortAsync method.
    /// </summary>
public ValueTask OnAbortAsync(long txnId, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

        /// <summary>
    /// DisposeAsync method.
    /// </summary>
public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private sealed class UnboundedSnapshot(long txnId) : IReadSnapshot
    {
                /// <summary>
        /// Gets or sets the Snapshot Transaction Id.
        /// </summary>
public long SnapshotTransactionId => txnId;
                /// <summary>
        /// IsVisible method.
        /// </summary>
public bool IsVisible(long xmin, long xmax) => true;
                /// <summary>
        /// Dispose method.
        /// </summary>
public void Dispose() { }
    }
}
