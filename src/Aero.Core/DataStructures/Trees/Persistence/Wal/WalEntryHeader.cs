using System.Runtime.InteropServices;

namespace Aero.Core.DataStructures.Trees.Persistence.Wal;

/// <summary>
/// Represents a struct for WalEntryHeader.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WalEntryHeader
{
        /// <summary>
    /// Crc32.
    /// </summary>
public uint Crc32;
        /// <summary>
    /// TotalLength.
    /// </summary>
public int TotalLength;
        /// <summary>
    /// Lsn.
    /// </summary>
public Lsn Lsn;
        /// <summary>
    /// TransactionId.
    /// </summary>
public long TransactionId;
        /// <summary>
    /// Type.
    /// </summary>
public WalEntryType Type;
        /// <summary>
    /// PageId.
    /// </summary>
public long PageId;
        /// <summary>
    /// PageOffset.
    /// </summary>
public int PageOffset;
        /// <summary>
    /// ImageLength.
    /// </summary>
public int ImageLength;
        /// <summary>
    /// ReferenceLsn.
    /// </summary>
public Lsn ReferenceLsn;
}
