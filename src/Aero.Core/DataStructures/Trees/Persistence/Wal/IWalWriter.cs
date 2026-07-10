namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Defines an interface for IWalWriter.
/// </summary>
public interface IWalWriter : IAsyncDisposable
{
        /// <summary>
    /// AppendAsync method.
    /// </summary>
ValueTask<Lsn> AppendAsync(WalEntry entry, CancellationToken ct = default);
        /// <summary>
    /// FlushAsync method.
    /// </summary>
ValueTask FlushAsync(CancellationToken ct = default);
        /// <summary>
    /// Gets or sets the Next Lsn.
    /// </summary>
Lsn NextLsn { get; }
        /// <summary>
    /// Gets or sets the File Size.
    /// </summary>
long FileSize { get; }
}
