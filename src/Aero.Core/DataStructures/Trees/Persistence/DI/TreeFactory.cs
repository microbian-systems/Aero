using Aero.Core.DataStructures.Trees.Persistence.Interfaces;
using Aero.Core.DataStructures.Trees.Persistence.Serialization;
using Aero.Core.DataStructures.Trees.Persistence.Storage;
using Aero.Core.DataStructures.Trees.Persistence.Trees;

namespace Aero.Core.DataStructures.Trees.Persistence.DI;

/// <summary>
/// Represents a class for TreeFactory.
/// </summary>
public static class TreeFactory
{
        /// <summary>
    /// CreateMinHeap method.
    /// </summary>
public static IPriorityTree<T> CreateMinHeap<T>(IStorageBackend storage)
        where T : unmanaged, IComparable<T>
    {
        var serializer = new PrimitiveSerializer<T>();
        return new PersistentMinHeap<T>(storage, serializer);
    }

        /// <summary>
    /// CreateMaxHeap method.
    /// </summary>
public static IPriorityTree<T> CreateMaxHeap<T>(IStorageBackend storage)
        where T : unmanaged, IComparable<T>
    {
        var serializer = new PrimitiveSerializer<T>();
        return new PersistentMaxHeap<T>(storage, serializer);
    }

        /// <summary>
    /// CreateMinMaxHeap method.
    /// </summary>
public static IDoubleEndedPriorityTree<T> CreateMinMaxHeap<T>(IStorageBackend storage)
        where T : unmanaged, IComparable<T>
    {
        var serializer = new PrimitiveSerializer<T>();
        return new PersistentMinMaxHeap<T>(storage, serializer);
    }

        /// <summary>
    /// CreateBPlusTree method.
    /// </summary>
public static IOrderedTree<TKey> CreateBPlusTree<TKey, TValue>(IStorageBackend storage)
        where TKey : unmanaged, IComparable<TKey>
        where TValue : unmanaged
    {
        return new PersistentBPlusTree<TKey, TValue>(storage);
    }
}
