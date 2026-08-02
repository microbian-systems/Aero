using Aero.Core.DataStructures.Trees.Persistence.Concurrency;
using Aero.Core.DataStructures.Trees.Persistence.Storage;

namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Represents a class for WalStorageBackendFactory.
/// </summary>
public static class WalStorageBackendFactory
{
        /// <summary>
    /// CreateAsync method.
    /// </summary>
public static async ValueTask<IWalStorageBackend> CreateAsync(
        IStorageBackend inner,
        string walPath,
        IConcurrencyStrategy? concurrency = null,
        CancellationToken ct = default)
    {
        var walFile = new WalFile(walPath);
        var txnManager = new TransactionManager(walFile);

        if (walFile.FileSize > WalFile.HeaderSize)
        {
            var recovery = new RecoveryEngine(inner, walFile, walFile);
            await recovery.RecoverAsync(walFile.LastCheckpointLsn, ct);
        }

        return new WalStorageBackend(inner, walFile, txnManager, concurrency);
    }

        /// <summary>
    /// CreateWithMvccAsync method.
    /// </summary>
public static ValueTask<IWalStorageBackend> CreateWithMvccAsync(
        IStorageBackend inner,
        string walPath,
        CancellationToken ct = default) =>
        CreateAsync(inner, walPath, new MvccConcurrencyStrategy(), ct);

        /// <summary>
    /// CreateWithOccAsync method.
    /// </summary>
public static ValueTask<IWalStorageBackend> CreateWithOccAsync(
        IStorageBackend inner,
        string walPath,
        CancellationToken ct = default) =>
        CreateAsync(inner, walPath, new OccConcurrencyStrategy(), ct);
}
