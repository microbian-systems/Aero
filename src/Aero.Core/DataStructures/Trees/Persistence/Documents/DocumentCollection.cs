using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Aero.Core.DataStructures.Trees.Persistence.Heap;
using Aero.Core.DataStructures.Trees.Persistence.Indexes;
using Aero.Core.DataStructures.Trees.Persistence.Interfaces;
using Aero.Core.DataStructures.Trees.Persistence.Serialization;
using Aero.Core.DataStructures.Trees.Persistence.Wal;

namespace Aero.Core.DataStructures.Trees.Persistence.Documents;

public sealed class DocumentCollection<TDocument>(
    IWalStorageBackend storage,
    IHeapFile heap,
    IOrderedKeyValueTree<Guid, HeapAddress> primaryIndex,
    DocumentIndexRegistry<TDocument> indexRegistry,
    IDocumentSerializer<TDocument> serializer,
    Func<TDocument, Guid> idExtractor)
    : IDocumentCollection<TDocument>
    where TDocument : class
{
    private long _approximateCount;

    public long ApproximateCount => Interlocked.Read(ref _approximateCount);

    public async ValueTask<Guid> InsertAsync(TDocument document, CancellationToken ct = default)
    {
        await using var txn = await storage.BeginTransactionAsync(ct);

        try
        {
            var id = idExtractor(document);
            if (id == Guid.Empty) id = Guid.NewGuid();

            if (await primaryIndex.ContainsAsync(id, ct))
                throw new DuplicateKeyException(id);

            var bytes = serializer.Serialize(document);
            var address = await heap.WriteAsync(bytes, ct);

            await primaryIndex.InsertAsync(id, address, ct);

            foreach (var updater in indexRegistry.AllUpdaters)
                await updater.OnInsertAsync(id, document, ct);

            await txn.CommitAsync(ct);
            Interlocked.Increment(ref _approximateCount);
            return id;
        }
        catch
        {
            await txn.RollbackAsync(ct);
            throw;
        }
    }

    public async ValueTask<TDocument?> FindAsync(Guid id, CancellationToken ct = default)
    {
        var address = await primaryIndex.FindAsync(id, ct);
        if (address is null || address.Value.IsNull) return null;

        var bytes = await heap.ReadAsync(address.Value, ct);
        return serializer.Deserialize(bytes);
    }

    public async ValueTask<bool> UpdateAsync(
        Guid id, TDocument document, CancellationToken ct = default)
    {
        await using var txn = await storage.BeginTransactionAsync(ct);

        try
        {
            var oldAddress = await primaryIndex.FindAsync(id, ct);
            if (oldAddress is null || oldAddress.Value.IsNull) return false;

            var oldBytes = await heap.ReadAsync(oldAddress.Value, ct);
            var oldDoc = serializer.Deserialize(oldBytes);
            var newBytes = serializer.Serialize(document);
            var newAddress = await heap.UpdateAsync(oldAddress.Value, newBytes, ct);

            if (newAddress != oldAddress.Value)
                await primaryIndex.UpdateAsync(id, newAddress, ct);

            foreach (var updater in indexRegistry.AllUpdaters)
                await updater.OnUpdateAsync(id, oldDoc, document, ct);

            await txn.CommitAsync(ct);
            return true;
        }
        catch
        {
            await txn.RollbackAsync(ct);
            throw;
        }
    }

    public async ValueTask<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var txn = await storage.BeginTransactionAsync(ct);

        try
        {
            var address = await primaryIndex.FindAsync(id, ct);
            if (address is null || address.Value.IsNull) return false;

            var bytes = await heap.ReadAsync(address.Value, ct);
            var oldDoc = serializer.Deserialize(bytes);

            foreach (var updater in indexRegistry.AllUpdaters)
                await updater.OnDeleteAsync(id, oldDoc, ct);

            await primaryIndex.DeleteAsync(id, ct);
            await heap.DeleteAsync(address.Value, ct);

            await txn.CommitAsync(ct);
            Interlocked.Decrement(ref _approximateCount);
            return true;
        }
        catch
        {
            await txn.RollbackAsync(ct);
            throw;
        }
    }

    public IQueryable<TDocument> AsQueryable()
    {
        throw new NotImplementedException("Use the LINQ provider from the Linq namespace.");
    }

    public async IAsyncEnumerable<TDocument> ScanIndexAsync<TField>(
        Expression<Func<TDocument, TField>> fieldSelector,
        TField from,
        TField to,
        [EnumeratorCancellation] CancellationToken ct = default)
        where TField : unmanaged, IComparable<TField>
    {
        var fieldName = GetFieldName(fieldSelector);
        var def = indexRegistry.FindByField(fieldName)
                    ?? throw new ArgumentException(
                        $"Field '{fieldName}' is not indexed.", nameof(fieldSelector));

        var executor = indexRegistry.GetExecutor(def);

        await foreach (var docId in executor.ScanRangeAsync(from, to, ct))
        {
            var doc = await FindAsync(docId, ct);
            if (doc is not null) yield return doc;
        }
    }

    public async IAsyncEnumerable<TDocument> ScanAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var (_, data) in heap.ScanAllAsync(ct))
            yield return serializer.Deserialize(data);
    }

    private static string GetFieldName<TField>(Expression<Func<TDocument, TField>> expr) =>
        ((MemberExpression)expr.Body).Member.Name;
}
