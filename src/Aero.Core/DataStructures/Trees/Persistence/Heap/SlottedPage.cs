using System.Buffers.Binary;

namespace Aero.Core.DataStructures.Trees.Persistence.Heap;

/// <summary>
/// Represents a struct for SlottedPage.
/// </summary>
public ref struct SlottedPage(Span<byte> page)
{
    private readonly Span<byte> _page = page;
    private readonly int _pageSize = page.Length;

        /// <summary>
    /// Gets or sets the Page Lsn.
    /// </summary>
public ulong PageLsn
    {
        get => BinaryPrimitives.ReadUInt64LittleEndian(_page[HeapPageLayout.PageLsnOffset..]);
        set => BinaryPrimitives.WriteUInt64LittleEndian(_page[HeapPageLayout.PageLsnOffset..], value);
    }

        /// <summary>
    /// Gets or sets the Slot Count.
    /// </summary>
public ushort SlotCount
    {
        get => BinaryPrimitives.ReadUInt16LittleEndian(_page[HeapPageLayout.SlotCountOffset..]);
        set => BinaryPrimitives.WriteUInt16LittleEndian(_page[HeapPageLayout.SlotCountOffset..], value);
    }

        /// <summary>
    /// Gets or sets the Live Count.
    /// </summary>
public ushort LiveCount
    {
        get => BinaryPrimitives.ReadUInt16LittleEndian(_page[HeapPageLayout.LiveCountOffset..]);
        set => BinaryPrimitives.WriteUInt16LittleEndian(_page[HeapPageLayout.LiveCountOffset..], value);
    }

        /// <summary>
    /// Gets or sets the Free Space Start.
    /// </summary>
public ushort FreeSpaceStart
    {
        get => BinaryPrimitives.ReadUInt16LittleEndian(_page[HeapPageLayout.FreeSpaceOffset..]);
        set => BinaryPrimitives.WriteUInt16LittleEndian(_page[HeapPageLayout.FreeSpaceOffset..], value);
    }

        /// <summary>
    /// Gets or sets the Free Bytes.
    /// </summary>
public int FreeBytes =>
        _pageSize - FreeSpaceStart - (SlotCount * HeapPageLayout.SlotEntrySize);

        /// <summary>
    /// WriteRecord method.
    /// </summary>
public short WriteRecord(ReadOnlySpan<byte> data)
    {
        if (data.Length > FreeBytes)
            throw new InvalidOperationException(
                $"Insufficient space: need {data.Length}, have {FreeBytes}.");

        short slotIndex = FindDeadSlot();
        bool newSlot = slotIndex == -1;

        if (newSlot)
            slotIndex = (short)SlotCount;

        var dataOffset = (ushort)(_pageSize - FreeSpaceStart - data.Length);
        data.CopyTo(_page[dataOffset..]);

        var slotOffset = HeapPageLayout.SlotOffset(slotIndex);
        BinaryPrimitives.WriteUInt16LittleEndian(_page[slotOffset..], dataOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(_page[(slotOffset + 2)..], (ushort)data.Length);
        _page[slotOffset + 4] = HeapPageLayout.SlotLive;

        if (newSlot) SlotCount++;
        LiveCount++;
        FreeSpaceStart += (ushort)data.Length;

        return slotIndex;
    }

        /// <summary>
    /// ReadRecord method.
    /// </summary>
public ReadOnlySpan<byte> ReadRecord(short slotIndex)
    {
        var slotOffset = HeapPageLayout.SlotOffset(slotIndex);
        var flags = _page[slotOffset + 4];

        if (flags == HeapPageLayout.SlotDeleted)
            return ReadOnlySpan<byte>.Empty;

        var dataOffset = BinaryPrimitives.ReadUInt16LittleEndian(_page[slotOffset..]);
        var dataLength = BinaryPrimitives.ReadUInt16LittleEndian(_page[(slotOffset + 2)..]);

        return _page.Slice(dataOffset, dataLength);
    }

        /// <summary>
    /// DeleteRecord method.
    /// </summary>
public void DeleteRecord(short slotIndex)
    {
        var slotOffset = HeapPageLayout.SlotOffset(slotIndex);
        _page[slotOffset + 4] = HeapPageLayout.SlotDeleted;
        LiveCount--;
    }

        /// <summary>
    /// TryUpdateRecord method.
    /// </summary>
public bool TryUpdateRecord(short slotIndex, ReadOnlySpan<byte> newData)
    {
        var slotOffset = HeapPageLayout.SlotOffset(slotIndex);
        var dataLength = BinaryPrimitives.ReadUInt16LittleEndian(_page[(slotOffset + 2)..]);

        if (newData.Length > dataLength) return false;

        var dataOffset = BinaryPrimitives.ReadUInt16LittleEndian(_page[slotOffset..]);
        newData.CopyTo(_page[dataOffset..]);
        BinaryPrimitives.WriteUInt16LittleEndian(_page[(slotOffset + 2)..], (ushort)newData.Length);
        return true;
    }

        /// <summary>
    /// Compact method.
    /// </summary>
public int Compact()
    {
        var liveRecords = new List<(short SlotIndex, byte[] Data)>();

        for (short i = 0; i < SlotCount; i++)
        {
            var record = ReadRecord(i);
            if (!record.IsEmpty)
                liveRecords.Add((i, record.ToArray()));
        }

        var dataRegionStart = HeapPageLayout.HeaderSize + SlotCount * HeapPageLayout.SlotEntrySize;
        _page[dataRegionStart..].Clear();

        int freedBytes = FreeSpaceStart;

        FreeSpaceStart = 0;
        foreach (var (slotIndex, data) in liveRecords)
        {
            var dataOffset = (ushort)(_pageSize - FreeSpaceStart - data.Length);
            data.CopyTo(_page[dataOffset..]);

            var slotOffset = HeapPageLayout.SlotOffset(slotIndex);
            BinaryPrimitives.WriteUInt16LittleEndian(_page[slotOffset..], dataOffset);
            BinaryPrimitives.WriteUInt16LittleEndian(_page[(slotOffset + 2)..], (ushort)data.Length);

            FreeSpaceStart += (ushort)data.Length;
        }

        return freedBytes - FreeSpaceStart;
    }

        /// <summary>
    /// InitializePage method.
    /// </summary>
public static SlottedPage InitializePage(Span<byte> page)
    {
        page.Clear();
        page[HeapPageLayout.NodeTypeOffset] = HeapPageLayout.NodeType;
        var sp = new SlottedPage(page);
        sp.SlotCount = 0;
        sp.LiveCount = 0;
        sp.FreeSpaceStart = 0;
        return sp;
    }

    private short FindDeadSlot()
    {
        for (short i = 0; i < SlotCount; i++)
        {
            var slotOffset = HeapPageLayout.SlotOffset(i);
            if (_page[slotOffset + 4] == HeapPageLayout.SlotDeleted)
                return i;
        }
        return -1;
    }
}
