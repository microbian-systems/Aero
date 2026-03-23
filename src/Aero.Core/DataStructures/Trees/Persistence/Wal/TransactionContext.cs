namespace Aero.DataStructures.Trees.Persistence.Wal;

internal sealed class TransactionContext(
    long transactionId,
    Lsn beginLsn,
    IWalWriter walWriter,
    TransactionManager manager)
    : ITransactionContext
{
    private readonly Dictionary<long, ReadOnlyMemory<byte>> _dirtyPages = new();
    private readonly Dictionary<long, Lsn> _writeLsns = new();
    private readonly Dictionary<long, uint> _readSet = new();
    private bool _committed;
    private bool _aborted;
    private bool _disposed;

    public long TransactionId => transactionId;
    public Lsn BeginLsn => beginLsn;
    public bool IsCommitted => _committed;
    public bool IsAborted => _aborted;
    public IReadOnlyDictionary<long, ReadOnlyMemory<byte>> DirtyPages => _dirtyPages;
    public IReadOnlyDictionary<long, uint> ReadSet => _readSet;

    public void TrackRead(long pageId)
    {
        ThrowIfDisposed();
        if (!_readSet.ContainsKey(pageId))
        {
            _readSet[pageId] = 0;
        }
    }

    public void TrackRead(long pageId, uint version)
    {
        ThrowIfDisposed();
        if (!_readSet.ContainsKey(pageId))
        {
            _readSet[pageId] = version;
        }
    }

    public void TrackWrite(long pageId, ReadOnlyMemory<byte> beforeImage)
    {
        ThrowIfDisposed();

        if (!_dirtyPages.ContainsKey(pageId))
        {
            _dirtyPages[pageId] = beforeImage;
        }
    }

    public void RecordWriteLsn(long pageId, Lsn lsn)
    {
        _writeLsns[pageId] = lsn;
    }

    public async ValueTask CommitAsync(CancellationToken ct = default)
    {
        ThrowIfDisposed();

        if (_committed || _aborted)
            throw new InvalidOperationException("Transaction already completed.");

        var commitEntry = new WalEntry
        {
            Header =
            {
                Type = WalEntryType.Commit,
                TransactionId = transactionId,
            }
        };

        await walWriter.AppendAsync(commitEntry, ct);
        await walWriter.FlushAsync(ct);

        _committed = true;
        manager.Complete(transactionId);
    }

    public async ValueTask RollbackAsync(CancellationToken ct = default)
    {
        ThrowIfDisposed();

        if (_committed || _aborted)
            throw new InvalidOperationException("Transaction already completed.");

        foreach (var (pageId, beforeImage) in _dirtyPages.Reverse())
        {
            var clrEntry = new WalEntry
            {
                Header =
                {
                    Type = WalEntryType.Clr,
                    TransactionId = transactionId,
                    PageId = pageId,
                    ReferenceLsn = _writeLsns.GetValueOrDefault(pageId, Lsn.Zero),
                    ImageLength = beforeImage.Length,
                },
                BeforeImage = beforeImage,
                AfterImage = beforeImage,
            };

            await walWriter.AppendAsync(clrEntry, ct);
        }

        var abortEntry = new WalEntry
        {
            Header =
            {
                Type = WalEntryType.Abort,
                TransactionId = transactionId,
            }
        };

        await walWriter.AppendAsync(abortEntry, ct);
        await walWriter.FlushAsync(ct);

        _aborted = true;
        manager.Complete(transactionId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (!_committed && !_aborted)
        {
            await RollbackAsync();
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransactionContext));
    }
}
