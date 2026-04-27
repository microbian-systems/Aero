using Aero.Core.DataStructures.Trees.Persistence.Interfaces;

namespace Aero.Core.DataStructures.Trees.Persistence.Indexes;

public sealed class SecondaryIndexUpdater<TDocument, TField>(
    IOrderedKeyValueTree<CompositeKey<TField, Guid>, Guid> index,
    Func<TDocument, TField> extractor)
    : IIndexUpdater<TDocument>
    where TDocument : class
    where TField : unmanaged, IComparable<TField>
{
    public async ValueTask OnInsertAsync(Guid id, TDocument doc, CancellationToken ct)
    {
        var key = extractor(doc);
        await index.InsertAsync(new CompositeKey<TField, Guid>(key, id), id, ct);
    }

    public async ValueTask OnUpdateAsync(
        Guid id, TDocument old, TDocument updated, CancellationToken ct)
    {
        var oldKey = extractor(old);
        var newKey = extractor(updated);

        if (EqualityComparer<TField>.Default.Equals(oldKey, newKey))
            return;

        await index.DeleteAsync(new CompositeKey<TField, Guid>(oldKey, id), ct);
        await index.InsertAsync(new CompositeKey<TField, Guid>(newKey, id), id, ct);
    }

    public async ValueTask OnDeleteAsync(Guid id, TDocument doc, CancellationToken ct)
    {
        var key = extractor(doc);
        await index.DeleteAsync(new CompositeKey<TField, Guid>(key, id), ct);
    }
}

public sealed class UniqueIndexUpdater<TDocument, TField>(
    IOrderedKeyValueTree<TField, Guid> index,
    Func<TDocument, TField> extractor,
    string indexName)
    : IIndexUpdater<TDocument>
    where TDocument : class
    where TField : unmanaged, IComparable<TField>
{
    public async ValueTask OnInsertAsync(Guid id, TDocument doc, CancellationToken ct)
    {
        var key = extractor(doc);

        if (await index.ContainsAsync(key, ct))
            throw new UniqueConstraintViolationException(indexName, key.ToString()!);

        await index.InsertAsync(key, id, ct);
    }

    public async ValueTask OnUpdateAsync(
        Guid id, TDocument old, TDocument updated, CancellationToken ct)
    {
        var oldKey = extractor(old);
        var newKey = extractor(updated);

        if (EqualityComparer<TField>.Default.Equals(oldKey, newKey))
            return;

        if (await index.ContainsAsync(newKey, ct))
            throw new UniqueConstraintViolationException(indexName, newKey.ToString()!);

        await index.DeleteAsync(oldKey, ct);
        await index.InsertAsync(newKey, id, ct);
    }

    public async ValueTask OnDeleteAsync(Guid id, TDocument doc, CancellationToken ct)
    {
        var key = extractor(doc);
        await index.DeleteAsync(key, ct);
    }
}

public sealed class UniqueConstraintViolationException(string indexName, string value)
    : Exception($"Unique index '{indexName}' already contains value '{value}'.")
{
    public string IndexName { get; } = indexName;
    public string Value { get; } = value;
}
