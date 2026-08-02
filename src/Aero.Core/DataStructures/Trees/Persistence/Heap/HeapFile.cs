using System.Runtime.CompilerServices;
using Aero.Core.DataStructures.Trees.Persistence.Storage;

namespace Aero.Core.DataStructures.Trees.Persistence.Heap;

/// <summary>
/// Represents a class for HeapFile.
/// </summary>
public sealed class HeapFile : IHeapFile
{
    private readonly IStorageBackend _storage;
    private readonly FreeSpaceMap _freeSpaceMap;
    private bool _disposed;
    private const byte HeapPageType = 0x03;

        /// <summary>
    /// Gets or sets the Page Size.
    /// </summary>
public int PageSize => _storage.PageSize;
        /// <summary>
    /// Gets or sets the Page Count.
    /// </summary>
public long PageCount => _storage.PageCount;

        /// <summary>
    /// Initializes a new instance of the <see cref="HeapFile"/> class.
    /// </summary>
public HeapFile(IStorageBackend storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _freeSpaceMap = new FreeSpaceMap();
        RebuildFreeSpaceMapAsync().GetAwaiter().GetResult();
    }

    private async Task RebuildFreeSpaceMapAsync()
    {
        for (long pageId = 0; pageId < _storage.PageCount; pageId++)
        {
            try
            {
                var page = await _storage.ReadPageAsync(pageId);
                var freeBytes = GetFreeBytesFromPage(page.Span);
                if (freeBytes.HasValue)
                    _freeSpaceMap.Record(pageId, freeBytes.Value);
            }
            catch (PageNotFoundException)
            {
                continue;
            }
        }
    }

    private static int? GetFreeBytesFromPage(Span<byte> pageSpan)
    {
        if (pageSpan.Length <= HeapPageLayout.NodeTypeOffset)
            return null;
        if (pageSpan[HeapPageLayout.NodeTypeOffset] != HeapPageType)
            return null;
        
        var sp = new SlottedPage(pageSpan);
        return sp.FreeBytes;
    }

        /// <summary>
    /// WriteAsync method.
    /// </summary>
public async ValueTask<HeapAddress> WriteAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken ct = default)
    {
        ThrowIfDisposed();

        var requiredSpace = data.Length + HeapPageLayout.SlotEntrySize;
        var pageId = _freeSpaceMap.FindPage(requiredSpace);

        if (pageId == -1)
        {
            pageId = await _storage.AllocatePageAsync(ct);
            var newPage = new byte[PageSize];
            SlottedPage.InitializePage(newPage);
            await _storage.WritePageAsync(pageId, newPage, ct);
            _freeSpaceMap.Record(pageId, PageSize - HeapPageLayout.HeaderSize);
        }

        var page = await _storage.ReadPageAsync(pageId, ct);
        var (slotIndex, freeBytes) = WriteRecordToPage(page, data);
        await _storage.WritePageAsync(pageId, page, ct);
        _freeSpaceMap.Record(pageId, freeBytes);

        return new HeapAddress(pageId, slotIndex);
    }

    private static (short slotIndex, int freeBytes) WriteRecordToPage(Memory<byte> page, ReadOnlyMemory<byte> data)
    {
        var sp = new SlottedPage(page.Span);
        var slotIndex = sp.WriteRecord(data.Span);
        return (slotIndex, sp.FreeBytes);
    }

        /// <summary>
    /// ReadAsync method.
    /// </summary>
public async ValueTask<Memory<byte>> ReadAsync(
        HeapAddress address,
        CancellationToken ct = default)
    {
        ThrowIfDisposed();

        if (address.IsNull)
            throw new ArgumentException("Cannot read from null address.", nameof(address));

        var page = await _storage.ReadPageAsync(address.PageId, ct);
        var record = ReadRecordFromPage(page, address.SlotIndex);

        if (record.IsEmpty)
            throw new RecordDeletedException(address);

        return record.ToArray();
    }

    private static ReadOnlySpan<byte> ReadRecordFromPage(Memory<byte> page, short slotIndex)
    {
        var sp = new SlottedPage(page.Span);
        return sp.ReadRecord(slotIndex);
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async ValueTask DeleteAsync(
        HeapAddress address,
        CancellationToken ct = default)
    {
        ThrowIfDisposed();

        if (address.IsNull)
            return;

        var page = await _storage.ReadPageAsync(address.PageId, ct);
        var freeBytes = DeleteRecordFromPage(page, address.SlotIndex);
        await _storage.WritePageAsync(address.PageId, page, ct);
        _freeSpaceMap.Record(address.PageId, freeBytes);
    }

    private static int DeleteRecordFromPage(Memory<byte> page, short slotIndex)
    {
        var sp = new SlottedPage(page.Span);
        sp.DeleteRecord(slotIndex);
        return sp.FreeBytes;
    }

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public async ValueTask<HeapAddress> UpdateAsync(
        HeapAddress address,
        ReadOnlyMemory<byte> newData,
        CancellationToken ct = default)
    {
        ThrowIfDisposed();

        if (address.IsNull)
            return await WriteAsync(newData, ct);

        var page = await _storage.ReadPageAsync(address.PageId, ct);
        var (success, freeBytes) = TryUpdateRecordInPage(page, address.SlotIndex, newData);
        
        if (success)
        {
            await _storage.WritePageAsync(address.PageId, page, ct);
            return address;
        }

        freeBytes = DeleteRecordFromPage(page, address.SlotIndex);
        await _storage.WritePageAsync(address.PageId, page, ct);
        _freeSpaceMap.Record(address.PageId, freeBytes);

        return await WriteAsync(newData, ct);
    }

    private static (bool success, int freeBytes) TryUpdateRecordInPage(Memory<byte> page, short slotIndex, ReadOnlyMemory<byte> newData)
    {
        var sp = new SlottedPage(page.Span);
        var success = sp.TryUpdateRecord(slotIndex, newData.Span);
        return (success, sp.FreeBytes);
    }

        /// <summary>
    /// CompactPageAsync method.
    /// </summary>
public async ValueTask CompactPageAsync(
        long pageId,
        CancellationToken ct = default)
    {
        ThrowIfDisposed();

        var page = await _storage.ReadPageAsync(pageId, ct);
        var freeBytes = CompactPageInPlace(page);
        await _storage.WritePageAsync(pageId, page, ct);
        _freeSpaceMap.Record(pageId, freeBytes);
    }

    private static int CompactPageInPlace(Memory<byte> page)
    {
        var sp = new SlottedPage(page.Span);
        sp.Compact();
        return sp.FreeBytes;
    }

        /// <summary>
    /// ScanAllAsync method.
    /// </summary>
public async IAsyncEnumerable<(HeapAddress Address, Memory<byte> Data)> ScanAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ThrowIfDisposed();

        foreach (var pageId in _freeSpaceMap.AllPageIds)
        {
            ct.ThrowIfCancellationRequested();

            Memory<byte> page;
            try
            {
                page = await _storage.ReadPageAsync(pageId, ct);
            }
            catch (PageNotFoundException)
            {
                continue;
            }

            var records = ExtractAllRecords(page);
            foreach (var (slotIndex, data) in records)
            {
                yield return (new HeapAddress(pageId, slotIndex), data);
            }
        }
    }

    private static List<(short slotIndex, Memory<byte> data)> ExtractAllRecords(Memory<byte> page)
    {
        var result = new List<(short, Memory<byte>)>();
        var sp = new SlottedPage(page.Span);

        for (short slotIndex = 0; slotIndex < sp.SlotCount; slotIndex++)
        {
            var record = sp.ReadRecord(slotIndex);
            if (!record.IsEmpty)
            {
                result.Add((slotIndex, record.ToArray()));
            }
        }

        return result;
    }

        /// <summary>
    /// DisposeAsync method.
    /// </summary>
public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _storage.FlushAsync();
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HeapFile));
    }
}
