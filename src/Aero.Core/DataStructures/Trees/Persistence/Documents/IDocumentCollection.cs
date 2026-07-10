using System.Linq.Expressions;

namespace Aero.Core.DataStructures.Trees.Persistence.Documents;

/// <summary>
/// Defines an interface for IDocumentCollection.
/// </summary>
public interface IDocumentCollection<TDocument> where TDocument : class
{
        /// <summary>
    /// InsertAsync method.
    /// </summary>
ValueTask<Guid> InsertAsync(
        TDocument document,
        CancellationToken ct = default);

        /// <summary>
    /// FindAsync method.
    /// </summary>
ValueTask<TDocument?> FindAsync(
        Guid id,
        CancellationToken ct = default);

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
ValueTask<bool> UpdateAsync(
        Guid id,
        TDocument document,
        CancellationToken ct = default);

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
ValueTask<bool> DeleteAsync(
        Guid id,
        CancellationToken ct = default);

        /// <summary>
    /// AsQueryable method.
    /// </summary>
IQueryable<TDocument> AsQueryable();

        /// <summary>
    /// ScanIndexAsync method.
    /// </summary>
IAsyncEnumerable<TDocument> ScanIndexAsync<TField>(
        Expression<Func<TDocument, TField>> fieldSelector,
        TField from,
        TField to,
        CancellationToken ct = default)
        where TField : unmanaged, IComparable<TField>;

        /// <summary>
    /// ScanAllAsync method.
    /// </summary>
IAsyncEnumerable<TDocument> ScanAllAsync(
        CancellationToken ct = default);

        /// <summary>
    /// Gets or sets the Approximate Count.
    /// </summary>
long ApproximateCount { get; }
}
