using Aero.Core.DataStructures.Trees.Persistence.Wal;

namespace Aero.Core.DataStructures.Trees.Persistence.Concurrency;

public sealed class NoIsolationStrategy : IConcurrencyStrategy
{
    public IsolationLevel Level => IsolationLevel.ReadCommitted;

    public ValueTask<IReadSnapshot> BeginReadAsync(long txnId, CancellationToken ct = default)
    {
        return ValueTask.FromResult<IReadSnapshot>(new UnboundedSnapshot(txnId));
    }

    public ValueTask BeginWriteAsync(long txnId, long pageId, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ValidateAsync(ITransactionContext txn, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnCommitAsync(long txnId, Lsn commitLsn, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask OnAbortAsync(long txnId, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private sealed class UnboundedSnapshot(long txnId) : IReadSnapshot
    {
        public long SnapshotTransactionId => txnId;
        public bool IsVisible(long xmin, long xmax) => true;
        public void Dispose() { }
    }
}
