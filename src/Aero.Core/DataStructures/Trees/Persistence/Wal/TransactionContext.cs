namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

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

        /// <summary>
    /// Gets or sets the Transaction Id.
    /// </summary>
public long TransactionId => transactionId;
        /// <summary>
    /// Gets or sets the Begin Lsn.
    /// </summary>
public Lsn BeginLsn => beginLsn;
        /// <summary>
    /// Gets or sets the Is Committed.
    /// </summary>
public bool IsCommitted => _committed;
        /// <summary>
    /// Gets or sets the Is Aborted.
    /// </summary>
public bool IsAborted => _aborted;
        /// <summary>
    /// Gets or sets the Dirty Pages.
    /// </summary>
public IReadOnlyDictionary<long, ReadOnlyMemory<byte>> DirtyPages => _dirtyPages;
        /// <summary>
    /// Gets or sets the Read Set.
    /// </summary>
public IReadOnlyDictionary<long, uint> ReadSet => _readSet;

        /// <summary>
    /// TrackRead method.
    /// </summary>
public void TrackRead(long pageId)
    {
        ThrowIfDisposed();
        if (!_readSet.ContainsKey(pageId))
        {
            _readSet[pageId] = 0;
        }
    }

        /// <summary>
    /// TrackRead method.
    /// </summary>
public void TrackRead(long pageId, uint version)
    {
        ThrowIfDisposed();
        if (!_readSet.ContainsKey(pageId))
        {
            _readSet[pageId] = version;
        }
    }

        /// <summary>
    /// TrackWrite method.
    /// </summary>
public void TrackWrite(long pageId, ReadOnlyMemory<byte> beforeImage)
    {
        ThrowIfDisposed();

        if (!_dirtyPages.ContainsKey(pageId))
        {
            _dirtyPages[pageId] = beforeImage;
        }
    }

        /// <summary>
    /// RecordWriteLsn method.
    /// </summary>
public void RecordWriteLsn(long pageId, Lsn lsn)
    {
        _writeLsns[pageId] = lsn;
    }

        /// <summary>
    /// CommitAsync method.
    /// </summary>
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

        /// <summary>
    /// RollbackAsync method.
    /// </summary>
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

        /// <summary>
    /// DisposeAsync method.
    /// </summary>
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
