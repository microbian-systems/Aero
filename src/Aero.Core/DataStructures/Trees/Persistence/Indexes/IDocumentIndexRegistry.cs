using Aero.Core.DataStructures.Trees.Persistence.Interfaces;

namespace Aero.Core.DataStructures.Trees.Persistence.Indexes;

/// <summary>
/// Defines an interface for IDocumentIndexRegistry.
/// </summary>
public interface IDocumentIndexRegistry<TDocument>
    where TDocument : class
{
        /// <summary>
    /// FindByField method.
    /// </summary>
IndexDefinition? FindByField(string fieldName);
        /// <summary>
    /// Gets or sets the All Indexes.
    /// </summary>
IReadOnlyList<IndexDefinition> AllIndexes { get; }
        /// <summary>
    /// GetExecutor method.
    /// </summary>
IIndexExecutor<TDocument> GetExecutor(IndexDefinition definition);
    
        /// <summary>
    /// Register method.
    /// </summary>
void Register<TField>(
        IndexDefinition<TDocument, TField> definition,
        IOrderedKeyValueTree<CompositeKey<TField, Guid>, Guid> tree)
        where TField : unmanaged, IComparable<TField>;

        /// <summary>
    /// RegisterUnique method.
    /// </summary>
void RegisterUnique<TField>(
        IndexDefinition<TDocument, TField> definition,
        IOrderedKeyValueTree<TField, Guid> tree)
        where TField : unmanaged, IComparable<TField>;
}

/// <summary>
/// Defines an interface for IIndexExecutor.
/// </summary>
public interface IIndexExecutor<TDocument>
    where TDocument : class
{
        /// <summary>
    /// Gets or sets the Definition.
    /// </summary>
IndexDefinition Definition { get; }
    
        /// <summary>
    /// LookupAsync method.
    /// </summary>
IAsyncEnumerable<Guid> LookupAsync(
        object fieldValue,
        CancellationToken ct = default);
    
        /// <summary>
    /// ScanRangeAsync method.
    /// </summary>
IAsyncEnumerable<Guid> ScanRangeAsync(
        object? from,
        object? to,
        CancellationToken ct = default);
}
