namespace Aero.DataStructures.Trees.Persistence.Concurrency;

public static class TransactionContext
{
    private static readonly AsyncLocal<long> _current = new();

    public static long Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }

    public static IDisposable Scope(long txnId)
    {
        var previous = _current.Value;
        _current.Value = txnId;
        return new RestoreScope(previous);
    }

    private sealed class RestoreScope(long previous) : IDisposable
    {
        private bool _disposed;

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
