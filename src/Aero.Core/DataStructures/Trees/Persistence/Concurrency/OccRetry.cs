using Aero.Core.DataStructures.Trees.Persistence.Wal;

namespace Aero.Core.DataStructures.Trees.Persistence.Concurrency;

/// <summary>
/// Represents a class for MaxRetriesExceededException.
/// </summary>
public sealed class MaxRetriesExceededException(int maxRetries)
    : Exception($"Operation failed after {maxRetries} attempts due to conflicts.")
{
        /// <summary>
    /// Gets or sets the Max Retries.
    /// </summary>
public int MaxRetries { get; } = maxRetries;
}

/// <summary>
/// Represents a class for OccRetry.
/// </summary>
public static class OccRetry
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public static async ValueTask<T> ExecuteAsync<T>(
        IWalStorageBackend backend,
        Func<ITransactionContext, ValueTask<T>> operation,
        int maxRetries = 5,
        CancellationToken ct = default)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            await using var txn = await backend.BeginTransactionAsync(ct);
            try
            {
                var result = await operation(txn);
                await txn.CommitAsync(ct);
                return result;
            }
            catch (ConflictException) when (attempt < maxRetries)
            {
                await txn.RollbackAsync(ct);
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, attempt)), ct);
            }
        }
        throw new MaxRetriesExceededException(maxRetries);
    }

        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public static async ValueTask ExecuteAsync(
        IWalStorageBackend backend,
        Func<ITransactionContext, ValueTask> operation,
        int maxRetries = 5,
        CancellationToken ct = default)
    {
        await ExecuteAsync<object?>(backend, async txn =>
        {
            await operation(txn);
            return null;
        }, maxRetries, ct);
    }
}
