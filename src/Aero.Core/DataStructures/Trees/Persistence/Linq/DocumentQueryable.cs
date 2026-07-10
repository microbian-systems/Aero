using System.Collections;
using System.Linq.Expressions;

namespace Aero.Core.DataStructures.Trees.Persistence.Linq;

/// <summary>
/// Represents a class for DocumentQueryable.
/// </summary>
public sealed class DocumentQueryable<TDocument> : IQueryable<TDocument>
    where TDocument : class
{
    internal DocumentQueryProvider<TDocument> TypedProvider { get; }

        /// <summary>
    /// Initializes a new instance of the <see cref="DocumentQueryable"/> class.
    /// </summary>
public DocumentQueryable(DocumentQueryProvider<TDocument> provider)
    {
        TypedProvider = provider;
        Expression = Expression.Constant(this);
    }

    internal DocumentQueryable(
        DocumentQueryProvider<TDocument> provider,
        Expression expression)
    {
        TypedProvider = provider;
        Expression = expression;
    }

        /// <summary>
    /// Gets or sets the Element Type.
    /// </summary>
public Type ElementType => typeof(TDocument);
        /// <summary>
    /// Gets or sets the Expression.
    /// </summary>
public Expression Expression { get; }
        /// <summary>
    /// Gets or sets the Provider.
    /// </summary>
public IQueryProvider Provider => TypedProvider;

        /// <summary>
    /// GetEnumerator method.
    /// </summary>
public IEnumerator<TDocument> GetEnumerator() =>
        TypedProvider
            .Execute<IEnumerable<TDocument>>(Expression)
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
