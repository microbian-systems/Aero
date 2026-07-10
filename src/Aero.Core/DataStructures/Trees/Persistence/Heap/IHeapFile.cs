namespace Aero.Core.DataStructures.Trees.Persistence.Heap;

/// <summary>
/// Defines an interface for IHeapFile.
/// </summary>
public interface IHeapFile : IAsyncDisposable
{
        /// <summary>
    /// WriteAsync method.
    /// </summary>
ValueTask<HeapAddress> WriteAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken ct = default);

        /// <summary>
    /// ReadAsync method.
    /// </summary>
ValueTask<Memory<byte>> ReadAsync(
        HeapAddress address,
        CancellationToken ct = default);

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
ValueTask DeleteAsync(
        HeapAddress address,
        CancellationToken ct = default);

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
ValueTask<HeapAddress> UpdateAsync(
        HeapAddress address,
        ReadOnlyMemory<byte> newData,
        CancellationToken ct = default);

        /// <summary>
    /// CompactPageAsync method.
    /// </summary>
ValueTask CompactPageAsync(
        long pageId,
        CancellationToken ct = default);

        /// <summary>
    /// ScanAllAsync method.
    /// </summary>
IAsyncEnumerable<(HeapAddress Address, Memory<byte> Data)> ScanAllAsync(
        CancellationToken ct = default);

        /// <summary>
    /// Gets or sets the Page Size.
    /// </summary>
int PageSize { get; }
        /// <summary>
    /// Gets or sets the Page Count.
    /// </summary>
long PageCount { get; }
}
