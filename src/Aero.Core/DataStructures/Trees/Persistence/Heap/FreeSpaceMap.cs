using System.Collections.Concurrent;

namespace Aero.Core.DataStructures.Trees.Persistence.Heap;

/// <summary>
/// Represents a class for FreeSpaceMap.
/// </summary>
public sealed class FreeSpaceMap
{
    private const int Quantum = 32;
    private readonly ConcurrentDictionary<long, int> _freeBytes = new();

        /// <summary>
    /// Record method.
    /// </summary>
public void Record(long pageId, int freeBytes) =>
        _freeBytes[pageId] = (freeBytes / Quantum) * Quantum;

        /// <summary>
    /// FindPage method.
    /// </summary>
public long FindPage(int requiredBytes)
    {
        foreach (var (pageId, free) in _freeBytes)
            if (free >= requiredBytes)
                return pageId;
        return -1;
    }

        /// <summary>
    /// Remove method.
    /// </summary>
public void Remove(long pageId) => _freeBytes.TryRemove(pageId, out _);

        /// <summary>
    /// Gets or sets the All Page Ids.
    /// </summary>
public IEnumerable<long> AllPageIds => _freeBytes.Keys;
}
