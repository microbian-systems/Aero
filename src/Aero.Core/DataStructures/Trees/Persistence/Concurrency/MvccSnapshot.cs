namespace Aero.DataStructures.Trees.Persistence.Concurrency;

public sealed class MvccSnapshot(
    long snapshotTxnId,
    CommitTable commitTable,
    IReadOnlySet<long> inProgressAtSnapshot)
    : IReadSnapshot
{
    public long SnapshotTransactionId => snapshotTxnId;

    public bool IsVisible(long xmin, long xmax)
    {
        if (!commitTable.IsCommitted(xmin)) return false;
        if (inProgressAtSnapshot.Contains(xmin)) return false;

        if (xmax == 0) return true;

        if (inProgressAtSnapshot.Contains(xmax)) return true;
        if (!commitTable.IsCommitted(xmax)) return true;

        return false;
    }

    public void Dispose() { }
}
