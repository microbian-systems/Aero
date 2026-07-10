using System.Linq.Expressions;
using Aero.Core.Entities;

namespace Aero.Core.Data;

/// <summary>
/// Defines an interface for IActiveRecord.
/// </summary>
public interface IActiveRecord<T, in TKey> 
    where T: IEntity<TKey> 
    where TKey : IComparable, IEquatable<TKey>
{
        /// <summary>
    /// Get method.
    /// </summary>
Task<T> Get(TKey key);
        /// <summary>
    /// Insert method.
    /// </summary>
Task Insert(T record);
        /// <summary>
    /// Update method.
    /// </summary>
Task Update(T record);
        /// <summary>
    /// Delete method.
    /// </summary>
Task Delete(TKey id);
        /// <summary>
    /// Delete method.
    /// </summary>
Task Delete(T record);
        /// <summary>
    /// Find method.
    /// </summary>
Task<IEnumerable<T>> Find(Expression<Func<T, bool>> expression);
}

/// <summary>
/// Represents a class for ActiveRecord.
/// </summary>
public abstract class ActiveRecord<T, TKey>(ILogger<ActiveRecord<T, TKey>> log) 
    : IActiveRecord<T, TKey> where T : IEntity<TKey> where TKey : IEquatable<TKey>, IComparable
{
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<ActiveRecord<T,TKey>> log = log;

        /// <summary>
    /// Get method.
    /// </summary>
public abstract Task<T> Get(TKey key);
        /// <summary>
    /// Insert method.
    /// </summary>
public abstract Task Insert(T record);
        /// <summary>
    /// Update method.
    /// </summary>
public abstract Task Update(T record);
        /// <summary>
    /// Delete method.
    /// </summary>
public abstract Task Delete(TKey id);
        /// <summary>
    /// Delete method.
    /// </summary>
public abstract Task Delete(T record);
        /// <summary>
    /// Find method.
    /// </summary>
public abstract Task<IEnumerable<T>> Find(Expression<Func<T, bool>> expression);
}