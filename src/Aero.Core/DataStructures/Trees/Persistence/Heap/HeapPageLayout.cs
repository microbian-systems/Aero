namespace Aero.Core.DataStructures.Trees.Persistence.Heap;

/// <summary>
/// Represents a class for HeapPageLayout.
/// </summary>
public static class HeapPageLayout
{
        /// <summary>
    /// NodeType.
    /// </summary>
public const byte NodeType = 0x03;

        /// <summary>
    /// PageLsnOffset.
    /// </summary>
public const int PageLsnOffset = 0;
        /// <summary>
    /// PageVersionOffset.
    /// </summary>
public const int PageVersionOffset = 8;
        /// <summary>
    /// NodeTypeOffset.
    /// </summary>
public const int NodeTypeOffset = 12;
        /// <summary>
    /// SlotCountOffset.
    /// </summary>
public const int SlotCountOffset = 16;
        /// <summary>
    /// LiveCountOffset.
    /// </summary>
public const int LiveCountOffset = 18;
        /// <summary>
    /// FreeSpaceOffset.
    /// </summary>
public const int FreeSpaceOffset = 20;
        /// <summary>
    /// HeaderSize.
    /// </summary>
public const int HeaderSize = 32;

        /// <summary>
    /// SlotEntrySize.
    /// </summary>
public const int SlotEntrySize = 5;

        /// <summary>
    /// SlotLive.
    /// </summary>
public const byte SlotLive = 0x00;
        /// <summary>
    /// SlotDeleted.
    /// </summary>
public const byte SlotDeleted = 0x01;

        /// <summary>
    /// SlotOffset method.
    /// </summary>
public static int SlotOffset(int slotIndex) =>
        HeaderSize + slotIndex * SlotEntrySize;

        /// <summary>
    /// MaxSlots method.
    /// </summary>
public static int MaxSlots(int pageSize) =>
        (pageSize - HeaderSize) / (SlotEntrySize + 1);

        /// <summary>
    /// FreeSpaceAvailable method.
    /// </summary>
public static int FreeSpaceAvailable(int pageSize, int currentFreeSpaceOffset, int slotCount) =>
        pageSize - currentFreeSpaceOffset - (slotCount * SlotEntrySize);
}
