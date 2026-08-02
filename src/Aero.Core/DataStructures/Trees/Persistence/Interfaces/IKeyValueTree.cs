namespace Aero.Core.DataStructures.Trees.Persistence.Interfaces;

/// <summary>
/// Defines an interface for IKeyValueTree.
/// </summary>
public interface IKeyValueTree<TKey, TValue> : ITree<TKey>
    where TKey : unmanaged, IComparable<TKey>
    where TValue : unmanaged
{
        /// <summary>
    /// InsertAsync method.
    /// </summary>
ValueTask InsertAsync(TKey key, TValue value, CancellationToken ct = default);
        /// <summary>
    /// DeleteAsync method.
    /// </summary>
ValueTask<bool> DeleteAsync(TKey key, CancellationToken ct = default);
        /// <summary>
    /// ContainsAsync method.
    /// </summary>
ValueTask<bool> ContainsAsync(TKey key, CancellationToken ct = default);
        /// <summary>
    /// TryGetAsync method.
    /// </summary>
ValueTask<(bool found, TValue value)> TryGetAsync(TKey key, CancellationToken ct = default);
        /// <summary>
    /// UpdateAsync method.
    /// </summary>
ValueTask<bool> UpdateAsync(TKey key, TValue newValue, CancellationToken ct = default);
        /// <summary>
    /// FindAsync method.
    /// </summary>
ValueTask<TValue?> FindAsync(TKey key, CancellationToken ct = default);
}

/// <summary>
/// Defines an interface for IOrderedKeyValueTree.
/// </summary>
public interface IOrderedKeyValueTree<TKey, TValue> : IKeyValueTree<TKey, TValue>, IOrderedTree<TKey>
    where TKey : unmanaged, IComparable<TKey>
    where TValue : unmanaged
{
        /// <summary>
    /// ScanWithValuesAsync method.
    /// </summary>
IAsyncEnumerable<(TKey Key, TValue Value)> ScanWithValuesAsync(TKey from, TKey to, CancellationToken ct = default);
        /// <summary>
    /// FindKeyAsync method.
    /// </summary>
ValueTask<TKey?> FindKeyAsync(TValue value, CancellationToken ct = default);
}
