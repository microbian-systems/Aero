using System.Linq.Expressions;
using Aero.Core.Entities;

namespace Aero.Core.Data.ActiveRecord;

/// <summary>
/// Represents a class for ActiveRecordExtensions.
/// </summary>
public static class ActiveRecordExtensions
{
        /// <summary>
    /// GetById method.
    /// </summary>
public static T GetById<T, TKey>(this T entity, TKey id) 
        where T : IEntity<TKey> where TKey : IComparable, IEquatable<TKey>
    {
        
        return entity;
    }
}


/// <summary>
/// Represents a class for ActiveRecordEfCore.
/// </summary>
public class ActiveRecordEfCore<T>(object db, ILogger<ActiveRecordEfCore<T, long>> log) 
    : ActiveRecordEfCore<T, long>(db, log) 
    where T : IEntity<long>
{
        /// <summary>
    /// Get method.
    /// </summary>
public override async Task<T> Get(long key)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

        /// <summary>
    /// Insert method.
    /// </summary>
public override async Task Insert(T record)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

        /// <summary>
    /// Update method.
    /// </summary>
public override async Task Update(T record)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

        /// <summary>
    /// Delete method.
    /// </summary>
public override async Task Delete(long id)
    {
        throw new NotImplementedException();
    }

        /// <summary>
    /// Delete method.
    /// </summary>
public override async Task Delete(T record)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

        /// <summary>
    /// Find method.
    /// </summary>
public override async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> expression)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}

/// <summary>
/// Represents a class for ActiveRecordEfCore.
/// </summary>
public abstract class ActiveRecordEfCore<T, TKey>(object db, ILogger<ActiveRecordEfCore<T, TKey>> log) 
    : IActiveRecord<T, TKey> where T : IEntity<TKey> where TKey : IEquatable<TKey>, IComparable
{
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<ActiveRecordEfCore<T,TKey>> log = log;
        /// <summary>
    /// db.
    /// </summary>
protected readonly object db = db;

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