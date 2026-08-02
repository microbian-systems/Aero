namespace Aero.Core.DataStructures.Trees.Persistence.Concurrency;

/// <summary>
/// Represents a class for MvccSnapshot.
/// </summary>
public sealed class MvccSnapshot(
    long snapshotTxnId,
    CommitTable commitTable,
    IReadOnlySet<long> inProgressAtSnapshot)
    : IReadSnapshot
{
        /// <summary>
    /// Gets or sets the Snapshot Transaction Id.
    /// </summary>
public long SnapshotTransactionId => snapshotTxnId;

        /// <summary>
    /// IsVisible method.
    /// </summary>
public bool IsVisible(long xmin, long xmax)
    {
        if (!commitTable.IsCommitted(xmin)) return false;
        if (inProgressAtSnapshot.Contains(xmin)) return false;

        if (xmax == 0) return true;

        if (inProgressAtSnapshot.Contains(xmax)) return true;
        if (!commitTable.IsCommitted(xmax)) return true;

        return false;
    }

        /// <summary>
    /// Dispose method.
    /// </summary>
public void Dispose() { }
}
