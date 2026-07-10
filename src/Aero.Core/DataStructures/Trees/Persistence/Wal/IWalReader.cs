namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Defines an interface for IWalReader.
/// </summary>
public interface IWalReader : IAsyncDisposable
{
        /// <summary>
    /// ReadFromAsync method.
    /// </summary>
IAsyncEnumerable<WalEntry> ReadFromAsync(Lsn startLsn, CancellationToken ct = default);
        /// <summary>
    /// ReadAllAsync method.
    /// </summary>
IAsyncEnumerable<WalEntry> ReadAllAsync(CancellationToken ct = default);
}
