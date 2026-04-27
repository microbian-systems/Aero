namespace Aero.Core.DataStructures.Trees.Persistence.Concurrency;

public sealed class ConflictException(long transactionId, long conflictingPageId)
    : Exception($"Transaction {transactionId} conflicts on page {conflictingPageId}.")
{
    public long TransactionId { get; } = transactionId;
    public long ConflictingPageId { get; } = conflictingPageId;
}
