namespace Aero.Core.DataStructures.Trees.Persistence.Heap;

public sealed class RecordDeletedException(HeapAddress address)
    : Exception($"Record at address ({address.PageId}, {address.SlotIndex}) has been deleted.")
{
    public HeapAddress Address { get; } = address;
}

public sealed class DuplicateKeyException(Guid id) : Exception($"A document with ID '{id}' already exists.")
{
    public Guid Id { get; } = id;
}
