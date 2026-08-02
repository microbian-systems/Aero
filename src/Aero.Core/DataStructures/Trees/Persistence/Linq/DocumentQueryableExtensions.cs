using System.Linq.Expressions;
using Aero.Core.DataStructures.Trees.Persistence.Documents;
using Aero.Core.DataStructures.Trees.Persistence.Indexes;
using Aero.Core.DataStructures.Trees.Persistence.Linq.Planning;

namespace Aero.Core.DataStructures.Trees.Persistence.Linq;

/// <summary>
/// Represents a class for DocumentQueryableExtensions.
/// </summary>
public static class DocumentQueryableExtensions
{
        /// <summary>
    /// ToListAsync method.
    /// </summary>
public static async ValueTask<List<TDocument>> ToListAsync<TDocument>(
        this IQueryable<TDocument> source,
        CancellationToken ct = default)
        where TDocument : class
    {
        var results = new List<TDocument>();
        await foreach (var doc in source.ToAsyncEnumerable(ct))
            results.Add(doc);
        return results;
    }

        /// <summary>
    /// FirstOrDefaultAsync method.
    /// </summary>
public static async ValueTask<TDocument?> FirstOrDefaultAsync<TDocument>(
        this IQueryable<TDocument> source,
        CancellationToken ct = default)
        where TDocument : class
    {
        await foreach (var doc in source.ToAsyncEnumerable(ct))
            return doc;
        return default;
    }

        /// <summary>
    /// FirstAsync method.
    /// </summary>
public static async ValueTask<TDocument> FirstAsync<TDocument>(
        this IQueryable<TDocument> source,
        CancellationToken ct = default)
        where TDocument : class
    {
        await foreach (var doc in source.ToAsyncEnumerable(ct))
            return doc;
        throw new InvalidOperationException("Sequence contains no elements.");
    }

        /// <summary>
    /// SingleOrDefaultAsync method.
    /// </summary>
public static async ValueTask<TDocument?> SingleOrDefaultAsync<TDocument>(
        this IQueryable<TDocument> source,
        CancellationToken ct = default)
        where TDocument : class
    {
        TDocument? result = default;
        bool found = false;

        await foreach (var doc in source.ToAsyncEnumerable(ct))
        {
            if (found)
                throw new InvalidOperationException(
                    "Sequence contains more than one element.");
            result = doc;
            found = true;
        }

        return result;
    }

        /// <summary>
    /// SingleAsync method.
    /// </summary>
public static async ValueTask<TDocument> SingleAsync<TDocument>(
        this IQueryable<TDocument> source,
        CancellationToken ct = default)
        where TDocument : class
    {
        TDocument? result = default;
        bool found = false;

        await foreach (var doc in source.ToAsyncEnumerable(ct))
        {
            if (found)
                throw new InvalidOperationException(
                    "Sequence contains more than one element.");
            result = doc;
            found = true;
        }

        if (!found)
            throw new InvalidOperationException("Sequence contains no elements.");

        return result!;
    }

        /// <summary>
    /// CountAsync method.
    /// </summary>
public static async ValueTask<int> CountAsync<TDocument>(
        this IQueryable<TDocument> source,
        CancellationToken ct = default)
        where TDocument : class
    {
        int count = 0;
        await foreach (var _ in source.ToAsyncEnumerable(ct))
            count++;
        return count;
    }

        /// <summary>
    /// AnyAsync method.
    /// </summary>
public static async ValueTask<bool> AnyAsync<TDocument>(
        this IQueryable<TDocument> source,
        CancellationToken ct = default)
        where TDocument : class
    {
        await foreach (var _ in source.ToAsyncEnumerable(ct))
            return true;
        return false;
    }

        /// <summary>
    /// AnyAsync method.
    /// </summary>
public static async ValueTask<bool> AnyAsync<TDocument>(
        this IQueryable<TDocument> source,
        Expression<Func<TDocument, bool>> predicate,
        CancellationToken ct = default)
        where TDocument : class =>
        await source.Where(predicate).AnyAsync(ct);

        /// <summary>
    /// AllAsync method.
    /// </summary>
public static async ValueTask<bool> AllAsync<TDocument>(
        this IQueryable<TDocument> source,
        Expression<Func<TDocument, bool>> predicate,
        CancellationToken ct = default)
        where TDocument : class
    {
        var compiled = predicate.Compile();
        await foreach (var doc in source.ToAsyncEnumerable(ct))
            if (!compiled(doc)) return false;
        return true;
    }

        /// <summary>
    /// ToAsyncEnumerable method.
    /// </summary>
public static IAsyncEnumerable<TDocument> ToAsyncEnumerable<TDocument>(
        this IQueryable<TDocument> source,
        CancellationToken ct = default)
        where TDocument : class
    {
        if (source is DocumentQueryable<TDocument> dq)
            return dq.TypedProvider.ExecuteAsync(source.Expression, ct);

        throw new InvalidOperationException(
            $"ToAsyncEnumerable requires a {nameof(DocumentQueryable<TDocument>)}.");
    }

        /// <summary>
    /// AsQueryable method.
    /// </summary>
public static IQueryable<TDocument> AsQueryable<TDocument>(
        this IDocumentCollection<TDocument> collection,
        IDocumentIndexRegistry<TDocument> registry,
        IQueryDiagnostics? diagnostics = null)
        where TDocument : class
    {
        var provider = new DocumentQueryProvider<TDocument>(collection, registry, diagnostics);
        return new DocumentQueryable<TDocument>(provider);
    }
}
