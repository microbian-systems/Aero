namespace Aero.Core.DataStructures.Trees.Persistence.Concurrency;

/// <summary>
/// Represents a class for ConflictException.
/// </summary>
public sealed class ConflictException(long transactionId, long conflictingPageId)
    : Exception($"Transaction {transactionId} conflicts on page {conflictingPageId}.")
{
        /// <summary>
    /// Gets or sets the Transaction Id.
    /// </summary>
public long TransactionId { get; } = transactionId;
        /// <summary>
    /// Gets or sets the Conflicting Page Id.
    /// </summary>
public long ConflictingPageId { get; } = conflictingPageId;
}
