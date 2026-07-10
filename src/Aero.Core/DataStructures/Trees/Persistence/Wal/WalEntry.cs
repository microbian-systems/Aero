namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Represents a class for WalEntry.
/// </summary>
public sealed class WalEntry
{
        /// <summary>
    /// Header.
    /// </summary>
public WalEntryHeader Header;

        /// <summary>
    /// BeforeImage.
    /// </summary>
public ReadOnlyMemory<byte> BeforeImage;

        /// <summary>
    /// AfterImage.
    /// </summary>
public ReadOnlyMemory<byte> AfterImage;

        /// <summary>
    /// Gets or sets the Is Committed.
    /// </summary>
public bool IsCommitted => Header.Type == WalEntryType.Commit;
        /// <summary>
    /// Gets or sets the Is Aborted.
    /// </summary>
public bool IsAborted => Header.Type == WalEntryType.Abort;
        /// <summary>
    /// Gets or sets the Is Write.
    /// </summary>
public bool IsWrite => Header.Type == WalEntryType.Write;
        /// <summary>
    /// Gets or sets the Is Checkpoint.
    /// </summary>
public bool IsCheckpoint => Header.Type == WalEntryType.Checkpoint;
}
