using System.Collections.Concurrent;

namespace Aero.Core.DataStructures.Trees.Persistence.Concurrency;

/// <summary>
/// Represents a class for CommitTable.
/// </summary>
public sealed class CommitTable
{
    private readonly ConcurrentDictionary<long, CommitStatus> _entries = new();

        /// <summary>
    /// RecordCommit method.
    /// </summary>
public void RecordCommit(long txnId) =>
        _entries[txnId] = new CommitStatus(txnId, CommitState.Committed);

        /// <summary>
    /// RecordAbort method.
    /// </summary>
public void RecordAbort(long txnId) =>
        _entries[txnId] = new CommitStatus(txnId, CommitState.Aborted);

        /// <summary>
    /// GetState method.
    /// </summary>
public CommitState GetState(long txnId)
    {
        if (txnId == 0) return CommitState.Committed;
        return _entries.TryGetValue(txnId, out var status)
            ? status.State
            : CommitState.InProgress;
    }

        /// <summary>
    /// IsCommitted method.
    /// </summary>
public bool IsCommitted(long txnId) => GetState(txnId) == CommitState.Committed;
        /// <summary>
    /// IsAborted method.
    /// </summary>
public bool IsAborted(long txnId) => GetState(txnId) == CommitState.Aborted;
        /// <summary>
    /// IsInProgress method.
    /// </summary>
public bool IsInProgress(long txnId) => GetState(txnId) == CommitState.InProgress;

        /// <summary>
    /// Evict method.
    /// </summary>
public void Evict(long txnId) => _entries.TryRemove(txnId, out _);
}

/// <summary>
/// Defines an enumeration for CommitState.
/// </summary>
public enum CommitState : byte
{
    InProgress = 0,
    Committed = 1,
    Aborted = 2,
}

/// <summary>
/// Represents a record for CommitStatus.
/// </summary>
public readonly record struct CommitStatus(long TxnId, CommitState State);
