using System.Collections;
using System.Linq.Expressions;
using AeroDB.Sable;

namespace Aero.Auth.Sable.Tests.TestSupport;

/// <summary>
/// Small explicit query fake for service tests. It supports only the query
/// operations exercised by refresh-token persistence and rejects any widening
/// use so a real Sable integration test remains the translation authority.
/// </summary>
internal sealed class TestSableQueryable<T> : ISableQueryable<T>
    where T : class
{
    private readonly IQueryable<T> values;
    private readonly Expression expression;
    private readonly Tracker tracker;
    private readonly IQueryProvider provider;

    public TestSableQueryable(IReadOnlyList<T> values)
        : this(values.AsQueryable(), values.AsQueryable().Expression, new Tracker())
    {
    }

    private TestSableQueryable(IQueryable<T> values, Expression expression, Tracker tracker)
    {
        this.values = values;
        this.expression = expression;
        this.tracker = tracker;
        provider = new TestQueryProvider(this);
    }

    public int FirstOrDefaultCalls => tracker.FirstOrDefaultCalls;

    public int ToListCalls => tracker.ToListCalls;

    public CancellationToken LastCancellationToken => tracker.LastCancellationToken;

    public Type ElementType => typeof(T);

    public Expression Expression => expression;

    public IQueryProvider Provider => provider;

    public IEnumerator<T> GetEnumerator() => Evaluate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Task<List<T>> ToListAsync(CancellationToken ct = default)
    {
        tracker.ToListCalls++;
        tracker.LastCancellationToken = ct;
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Evaluate().ToList());
    }

    public Task<T?> FirstOrDefaultAsync(CancellationToken ct = default)
    {
        tracker.FirstOrDefaultCalls++;
        tracker.LastCancellationToken = ct;
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Evaluate().FirstOrDefault());
    }

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        tracker.FirstOrDefaultCalls++;
        tracker.LastCancellationToken = ct;
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(predicate);
        return Task.FromResult(Evaluate().FirstOrDefault(predicate));
    }

    public Task<T?> SingleOrDefaultAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Evaluate().SingleOrDefault());
    }

    public Task<int> CountAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Evaluate().Count());
    }

    public Task<bool> AnyAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Evaluate().Any());
    }

    public Task<decimal> SumAsync(Expression<Func<T, decimal>> selector, CancellationToken ct = default) => Unsupported<decimal>();
    public Task<decimal> MinAsync(Expression<Func<T, decimal>> selector, CancellationToken ct = default) => Unsupported<decimal>();
    public Task<decimal> MaxAsync(Expression<Func<T, decimal>> selector, CancellationToken ct = default) => Unsupported<decimal>();
    public Task<decimal> AverageAsync(Expression<Func<T, decimal>> selector, CancellationToken ct = default) => Unsupported<decimal>();
    public ISableQueryable<T> Stats(out QueryStatistics stats) { stats = new QueryStatistics(); return this; }
    public SableCommand ToCommand() => throw new NotSupportedException("Test fake does not translate SurrealQL.");
    public ILinkedSableQueryable<T, TTarget> Link<TTarget>(Expression<Func<T, object?>> fkSelector) where TTarget : class => throw new NotSupportedException();
    public ILinkedSableQueryable<T, TTarget> Join<TTarget>(Expression<Func<T, object?>> fkSelector) where TTarget : class => throw new NotSupportedException();
    public ISableQueryable<T> Where<TTarget>(Expression<Func<T, TTarget, bool>> predicate) where TTarget : class => throw new NotSupportedException();
    public ISableQueryable<T> Where<TTarget1, TTarget2>(Expression<Func<T, TTarget1, TTarget2, bool>> predicate) where TTarget1 : class where TTarget2 : class => throw new NotSupportedException();
    public ISableQueryable<T> Where<TTarget1, TTarget2, TTarget3>(Expression<Func<T, TTarget1, TTarget2, TTarget3, bool>> predicate) where TTarget1 : class where TTarget2 : class where TTarget3 : class => throw new NotSupportedException();
    public ISableQueryable<T> Fetch(Expression<Func<T, object?>> property) => throw new NotSupportedException();
    public ISableQueryable<T> IncludeBatch<TProperty, TInclude>(Expression<Func<T, TProperty>> property, Action<TInclude> callback) where TInclude : class => throw new NotSupportedException();
    public ISableQueryable<T> IncludeBatch<TKey, TInclude>(Expression<Func<T, TKey>> key, IDictionary<TKey, TInclude> dictionary) where TInclude : class => throw new NotSupportedException();

    private IQueryable<T> Evaluate() => values.Provider.CreateQuery<T>(expression);

    private static Task<TValue> Unsupported<TValue>() => Task.FromException<TValue>(new NotSupportedException("Operation is outside this focused test fake."));

    private sealed class Tracker
    {
        public int FirstOrDefaultCalls { get; set; }
        public int ToListCalls { get; set; }
        public CancellationToken LastCancellationToken { get; set; }
    }

    private sealed class TestQueryProvider(TestSableQueryable<T> owner) : IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => CreateQuery<T>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (typeof(TElement) != typeof(T))
            {
                throw new NotSupportedException("This focused fake supports only the refresh-token element type.");
            }

            return (IQueryable<TElement>)(object)new TestSableQueryable<T>(owner.values, expression, owner.tracker);
        }

        public object? Execute(Expression expression) => owner.values.Provider.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => owner.values.Provider.Execute<TResult>(expression);
    }
}
