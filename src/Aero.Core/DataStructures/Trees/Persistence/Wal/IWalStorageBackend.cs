using Aero.Core.DataStructures.Trees.Persistence.Storage;

namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

public interface IWalStorageBackend : IStorageBackend
{
    ValueTask<ITransactionContext> BeginTransactionAsync(CancellationToken ct = default);
    Lsn LastCommittedLsn { get; }
}
