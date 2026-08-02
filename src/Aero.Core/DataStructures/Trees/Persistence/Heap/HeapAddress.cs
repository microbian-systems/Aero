using System.Runtime.InteropServices;

namespace Aero.Core.DataStructures.Trees.Persistence.Heap;

/// <summary>
/// Represents a record for HeapAddress.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct HeapAddress(long PageId, short SlotIndex)
    : IComparable<HeapAddress>
{
        /// <summary>
    /// Null.
    /// </summary>
public static readonly HeapAddress Null = new(-1, -1);
        /// <summary>
    /// Gets or sets the Is Null.
    /// </summary>
public bool IsNull => PageId == -1;

        /// <summary>
    /// CompareTo method.
    /// </summary>
public int CompareTo(HeapAddress other)
    {
        var cmp = PageId.CompareTo(other.PageId);
        return cmp != 0 ? cmp : SlotIndex.CompareTo(other.SlotIndex);
    }
}
