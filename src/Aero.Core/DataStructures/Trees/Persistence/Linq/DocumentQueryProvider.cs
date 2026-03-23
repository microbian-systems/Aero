using System.Linq.Expressions;
using Aero.DataStructures.Trees.Persistence.Documents;
using Aero.DataStructures.Trees.Persistence.Indexes;
using Aero.DataStructures.Trees.Persistence.Linq.Planning;
using Aero.DataStructures.Trees.Persistence.Linq.Translation;

namespace Aero.DataStructures.Trees.Persistence.Linq;

public sealed class DocumentQueryProvider<TDocument>(
    IDocumentCollection<TDocument> collection,
    IDocumentIndexRegistry<TDocument> registry,
    IQueryDiagnostics? diagnostics = null)
    : IQueryProvider
    where TDocument : class
{
    private readonly IDocumentIndexRegistry<TDocument> _registry = registry;
    private readonly QueryTranslator<TDocument> _translator = new(registry);
    private readonly QueryPlanner<TDocument> _planner = new(registry, diagnostics);

    public IQueryable CreateQuery(Expression expression) =>
        new DocumentQueryable<TDocument>(this, expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        if (typeof(TElement) != typeof(TDocument))
            throw new NotSupportedInQueryException(
                "Type change via Select is not supported.");
        return (IQueryable<TElement>)(object)
            new DocumentQueryable<TDocument>(this, expression);
    }

    public object? Execute(Expression expression) =>
        Execute<IEnumerable<TDocument>>(expression);

    public TResult Execute<TResult>(Expression expression)
    {
        var query = _translator.Translate(expression);
        var plan = _planner.Plan(query);
        var docs = plan.ExecuteAsync(collection, CancellationToken.None)
            .ToBlockingEnumerable()
            .ToList();
        return (TResult)(object)docs;
    }

    public IAsyncEnumerable<TDocument> ExecuteAsync(
        Expression expression,
        CancellationToken ct = default)
    {
        var query = _translator.Translate(expression);
        var plan = _planner.Plan(query);
        return plan.ExecuteAsync(collection, ct);
    }
}
