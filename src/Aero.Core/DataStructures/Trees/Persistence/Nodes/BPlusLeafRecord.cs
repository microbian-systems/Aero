using System.Runtime.InteropServices;

namespace Aero.Core.DataStructures.Trees.Persistence.Nodes;

/// <summary>
/// Represents a struct for BPlusLeafRecord.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BPlusLeafRecord<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
        /// <summary>
    /// Flags.
    /// </summary>
public RecordFlags Flags;
        /// <summary>
    /// XMin.
    /// </summary>
public long XMin;
        /// <summary>
    /// XMax.
    /// </summary>
public long XMax;
        /// <summary>
    /// Key.
    /// </summary>
public TKey Key;
        /// <summary>
    /// Value.
    /// </summary>
public TValue Value;

        /// <summary>
    /// Gets or sets the Is Live.
    /// </summary>
public bool IsLive => (Flags & RecordFlags.Deleted) == 0 && XMax == 0;
        /// <summary>
    /// Gets or sets the Is Deleted.
    /// </summary>
public bool IsDeleted => (Flags & RecordFlags.Deleted) != 0 || XMax != 0;

        /// <summary>
    /// MarkDeleted method.
    /// </summary>
public void MarkDeleted(long deleterTxnId)
    {
        XMax = deleterTxnId;
        Flags |= RecordFlags.Deleted;
        Value = default;
    }

        /// <summary>
    /// MarkDeleted method.
    /// </summary>
public void MarkDeleted()
    {
        Flags |= RecordFlags.Deleted;
        Value = default;
    }

        /// <summary>
    /// Tombstone method.
    /// </summary>
public static BPlusLeafRecord<TKey, TValue> Tombstone(TKey key) => new()
    {
        Flags = RecordFlags.Deleted,
        XMin = 0,
        XMax = 0,
        Key = key,
        Value = default
    };

        /// <summary>
    /// Live method.
    /// </summary>
public static BPlusLeafRecord<TKey, TValue> Live(TKey key, TValue value) => new()
    {
        Flags = RecordFlags.None,
        XMin = 0,
        XMax = 0,
        Key = key,
        Value = value
    };

        /// <summary>
    /// Create method.
    /// </summary>
public static BPlusLeafRecord<TKey, TValue> Create(TKey key, TValue value, long txnId) => new()
    {
        Flags = RecordFlags.None,
        XMin = txnId,
        XMax = 0,
        Key = key,
        Value = value
    };
}
