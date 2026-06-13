using Marten.Linq;

namespace Aero.Marten.Query;


/// <summary>
/// Scalar base query for entities with Snowflake IDs and returns the same type.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class AeroCompiledQueryScalar<T> : AeroCompiledQuery<T, T>
    where T : Entity, ISnowflakeEntity
{
}


/// <summary>
/// Base class for scalar compiled queries that return a different output type.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TOut">The output type.</typeparam>
public abstract class AeroCompiledQueryScalar<T, TOut> : AeroCompiledQuery<T, TOut>
    where T : Entity, ISnowflakeEntity
{
}

/// <summary>
/// Base class for compiled queries that return a list of entities with Snowflake IDs.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class AeroCompiledQueryList<T> : ICompiledQuery<T, IList<T>>
    where T : Entity, ISnowflakeEntity
{
    /// <summary>
    /// Defines the query logic using Marten's <see cref="IMartenQueryable{T}"/>.
    /// </summary>
    /// <returns>An expression defining the query.</returns>
    public abstract Expression<Func<IMartenQueryable<T>, IList<T>>> QueryIs();
}

/// <summary>
/// Base class for compiled queries with a specific output type for entities with long keys.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TOut">The output type.</typeparam>
public abstract class AeroCompiledQuery<T, TOut> : AeroCompiledQuery<T, TOut, long>
    where T : Entity, ISnowflakeEntity
{
}

/// <summary>
/// The base class for all Aero Marten compiled queries.
/// </summary>
/// <typeparam name="T">The entity type being queried.</typeparam>
/// <typeparam name="TOut">The type of the result returned by the query.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public abstract class AeroCompiledQuery<T, TOut, TKey> : ICompiledQuery<T, TOut>
    where T : IEntity<TKey>
    where TKey : IEquatable<TKey>, IComparable<TKey>
{
    /// <summary>
    /// Defines the query logic using Marten's <see cref="IMartenQueryable{T}"/>.
    /// </summary>
    /// <returns>An expression defining the query.</returns>
    public abstract Expression<Func<IMartenQueryable<T>, TOut>> QueryIs();
}

