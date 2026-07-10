using System.Linq.Expressions;
using Aero.Core.Entities;

namespace Aero.Core.Data;

/// <summary>
/// Represents a class for RepositoryBase.
/// </summary>
public abstract class RepositoryBase<T, TKey>(ILogger<RepositoryBase<T, TKey>> log) 
    : IWriteRepository<T, TKey>
    where T : EntityBase<TKey>, new()
    where TKey : IEquatable<TKey>
{
        /// <summary>
    /// log.
    /// </summary>
protected ILogger<RepositoryBase<T, TKey>> log = log;

        /// <summary>
    /// GetAll method.
    /// </summary>
public abstract IEnumerable<T> GetAll();
        /// <summary>
    /// FindById method.
    /// </summary>
public abstract T FindById(TKey id);
        /// <summary>
    /// Find method.
    /// </summary>
public abstract IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        /// <summary>
    /// GetAllAsync method.
    /// </summary>
public abstract Task<IEnumerable<T>> GetAllAsync();
        /// <summary>
    /// FindByIdAsync method.
    /// </summary>
public abstract Task<T> FindByIdAsync(TKey id);
        /// <summary>
    /// FindAsync method.
    /// </summary>
public abstract Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        /// <summary>
    /// AddAsync method.
    /// </summary>
public abstract Task<T> AddAsync(T entity);
        /// <summary>
    /// AddAsync method.
    /// </summary>
public abstract Task AddAsync(IEnumerable<T> entities);
        /// <summary>
    /// RemoveAllAsync method.
    /// </summary>
public abstract Task<long> RemoveAllAsync();
        /// <summary>
    /// RemoveAsync method.
    /// </summary>
public abstract Task RemoveAsync(IEnumerable<TKey> ids);
        /// <summary>
    /// RemoveAsync method.
    /// </summary>
public abstract Task RemoveAsync(TKey id);
        /// <summary>
    /// RemoveAsync method.
    /// </summary>
public abstract Task RemoveAsync(T entity);
        /// <summary>
    /// RemoveAsync method.
    /// </summary>
public abstract Task RemoveAsync(IEnumerable<T> entities);
        /// <summary>
    /// SaveAsync method.
    /// </summary>
public abstract Task SaveAsync(IEnumerable<T> entities);
        /// <summary>
    /// SaveAsync method.
    /// </summary>
public abstract Task<T> SaveAsync(T entity);
}