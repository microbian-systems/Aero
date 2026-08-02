namespace Aero.Core.DataStructures.Trees.Persistence.Heap;

/// <summary>
/// Represents a class for RecordDeletedException.
/// </summary>
public sealed class RecordDeletedException(HeapAddress address)
    : Exception($"Record at address ({address.PageId}, {address.SlotIndex}) has been deleted.")
{
        /// <summary>
    /// Gets or sets the Address.
    /// </summary>
public HeapAddress Address { get; } = address;
}

/// <summary>
/// Represents a class for DuplicateKeyException.
/// </summary>
public sealed class DuplicateKeyException(Guid id) : Exception($"A document with ID '{id}' already exists.")
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
public Guid Id { get; } = id;
}
