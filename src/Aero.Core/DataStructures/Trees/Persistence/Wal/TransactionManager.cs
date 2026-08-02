using System.Collections.Concurrent;

namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Represents a class for TransactionManager.
/// </summary>
public sealed class TransactionManager(IWalWriter wal) : IAsyncDisposable
{
    private readonly IWalWriter _wal = wal ?? throw new ArgumentNullException(nameof(wal));
    private long _nextTransactionId = 0;
    private readonly ConcurrentDictionary<long, ITransactionContext> _active = new();
    private bool _disposed;

        /// <summary>
    /// BeginAsync method.
    /// </summary>
public async ValueTask<ITransactionContext> BeginAsync(CancellationToken ct = default)
    {
        ThrowIfDisposed();

        var txnId = Interlocked.Increment(ref _nextTransactionId);

        var beginEntry = new WalEntry
        {
            Header =
            {
                Type = WalEntryType.Begin,
                TransactionId = txnId,
            }
        };

        var lsn = await _wal.AppendAsync(beginEntry, ct);

        var ctx = new TransactionContext(txnId, lsn, _wal, this);
        _active[txnId] = ctx;

        return ctx;
    }

    internal void Complete(long transactionId)
    {
        _active.TryRemove(transactionId, out _);
    }

        /// <summary>
    /// Gets or sets the Active Transaction Start Lsns.
    /// </summary>
public IEnumerable<Lsn> ActiveTransactionStartLsns =>
        _active.Values
            .Select(t => ((TransactionContext)t).BeginLsn)
            .OrderBy(l => l);

        /// <summary>
    /// DisposeAsync method.
    /// </summary>
public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var txn in _active.Values.ToArray())
        {
            await txn.RollbackAsync();
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransactionManager));
    }
}
