using Aero.Core.DataStructures.Trees.Persistence.Storage;

namespace Aero.Core.DataStructures.Trees.Persistence.DI;

/// <summary>
/// Represents a class for StorageBackendFactory.
/// </summary>
public static class StorageBackendFactory
{
        /// <summary>
    /// CreateInMemory method.
    /// </summary>
public static IStorageBackend CreateInMemory(int pageSize = 4096)
    {
        return new MemoryStorageBackend(pageSize);
    }

        /// <summary>
    /// CreateOnDisk method.
    /// </summary>
public static IStorageBackend CreateOnDisk(string path, int pageSize = 4096)
    {
        return new FileStorageBackend(path, pageSize);
    }

        /// <summary>
    /// CreateMmap method.
    /// </summary>
public static IZeroCopyStorageBackend CreateMmap(
        string path,
        long capacityBytes,
        int pageSize = 4096)
    {
        return new MmapStorageBackend(path, capacityBytes, pageSize);
    }
}
