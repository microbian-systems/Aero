using Aero.Core.DataStructures.Trees.Persistence.Storage;

namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Defines an interface for IWalStorageBackend.
/// </summary>
public interface IWalStorageBackend : IStorageBackend
{
        /// <summary>
    /// BeginTransactionAsync method.
    /// </summary>
ValueTask<ITransactionContext> BeginTransactionAsync(CancellationToken ct = default);
        /// <summary>
    /// Gets or sets the Last Committed Lsn.
    /// </summary>
Lsn LastCommittedLsn { get; }
}
