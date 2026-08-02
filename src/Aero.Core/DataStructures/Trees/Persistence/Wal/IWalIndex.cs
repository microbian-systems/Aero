namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Defines an interface for IWalIndex.
/// </summary>
public interface IWalIndex
{
        /// <summary>
    /// Record method.
    /// </summary>
void Record(Lsn lsn, long fileOffset);
        /// <summary>
    /// TryGetOffset method.
    /// </summary>
bool TryGetOffset(Lsn lsn, out long fileOffset);
        /// <summary>
    /// Gets or sets the Min Lsn.
    /// </summary>
Lsn MinLsn { get; }
        /// <summary>
    /// Gets or sets the Max Lsn.
    /// </summary>
Lsn MaxLsn { get; }
        /// <summary>
    /// TruncateBefore method.
    /// </summary>
void TruncateBefore(Lsn lsn);
}
