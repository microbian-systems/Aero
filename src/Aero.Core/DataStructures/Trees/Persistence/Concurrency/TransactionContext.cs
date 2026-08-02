namespace Aero.Core.DataStructures.Trees.Persistence.Concurrency;

/// <summary>
/// Represents a class for TransactionContext.
/// </summary>
public static class TransactionContext
{
    private static readonly AsyncLocal<long> _current = new();

        /// <summary>
    /// Gets or sets the Current.
    /// </summary>
public static long Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }

        /// <summary>
    /// Scope method.
    /// </summary>
public static IDisposable Scope(long txnId)
    {
        var previous = _current.Value;
        _current.Value = txnId;
        return new RestoreScope(previous);
    }

    private sealed class RestoreScope(long previous) : IDisposable
    {
        private bool _disposed;

                /// <summary>
        /// Dispose method.
        /// </summary>
public void Dispose()
        {
            if (!_disposed)
            {
                _current.Value = previous;
                _disposed = true;
            }
        }
    }
}
