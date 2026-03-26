using Aero.DataStructures.Trees.Persistence.Heap;
using Aero.DataStructures.Trees.Persistence.Indexes;
using Aero.DataStructures.Trees.Persistence.Serialization;

namespace Aero.DataStructures.Trees.Persistence.Documents;

public sealed class IndexRebuildService<TDocument>(
    IDocumentCollection<TDocument> collection,
    DocumentIndexRegistry<TDocument> registry,
    IDocumentSerializer<TDocument> serializer,
    IHeapFile heap)
    where TDocument : class
{
    private readonly IDocumentCollection<TDocument> _collection = collection;

    public async ValueTask RebuildAllAsync(
        IProgress<int>? progress = null,
        CancellationToken ct = default)
    {
        foreach (var index in registry.AllIndexes)
            await RebuildIndexAsync(index.FieldName, progress, ct);
    }

    public async ValueTask RebuildIndexAsync(
        string fieldName,
        IProgress<int>? progress = null,
        CancellationToken ct = default)
    {
        var def = registry.FindByField(fieldName)
                  ?? throw new ArgumentException($"No index found for field '{fieldName}'.");
        var updater = registry.GetUpdater(fieldName)
                      ?? throw new ArgumentException($"No updater found for field '{fieldName}'.");

        int count = 0;

        await foreach (var (_, data) in heap.ScanAllAsync(ct))
        {
            ct.ThrowIfCancellationRequested();

            var document = serializer.Deserialize(data);
            var id = ExtractId(document);

            await updater.OnInsertAsync(id, document, ct);
            progress?.Report(++count);
        }
    }

    private Guid ExtractId(TDocument document)
    {
        var idProp = typeof(TDocument).GetProperty("Id");
        if (idProp is not null && idProp.PropertyType == typeof(Guid))
            return (Guid)idProp.GetValue(document)!;
        return Guid.NewGuid();
    }
}
