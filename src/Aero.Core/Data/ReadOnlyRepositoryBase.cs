using System.Linq.Expressions;
using Aero.Core.Entities;

namespace Aero.Core.Data;

/// <summary>
/// Represents a class for ReadOnlyRepositoryBase.
/// </summary>
public abstract class ReadOnlyRepositoryBase<T, Tkey> : IReadOnlyRepository<T, Tkey> where T : IEntity<Tkey> where Tkey : IEquatable<Tkey>
{
        /// <summary>
    /// CountAsync method.
    /// </summary>
public abstract Task<long> CountAsync();
        /// <summary>
    /// ExistsAsync method.
    /// </summary>
public abstract Task<bool> ExistsAsync(Tkey id);
        /// <summary>
    /// GetAllAsync method.
    /// </summary>
public abstract Task<IEnumerable<T>> GetAllAsync();
        /// <summary>
    /// FindByIdAsync method.
    /// </summary>
public abstract Task<T> FindByIdAsync(Tkey id);
        /// <summary>
    /// FindAsync method.
    /// </summary>
public abstract Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        /// <summary>
    /// GetByIdAsync method.
    /// </summary>
public abstract Task<T> GetByIdAsync(Tkey id);
        /// <summary>
    /// GetByIdsAsync method.
    /// </summary>
public abstract Task<IReadOnlyCollection<T>> GetByIdsAsync(IEnumerable<Tkey> ids);
        /// <summary>
    /// GetAll method.
    /// </summary>
public abstract IEnumerable<T> GetAll();
        /// <summary>
    /// FindById method.
    /// </summary>
public abstract T FindById(Tkey id);
        /// <summary>
    /// Find method.
    /// </summary>
public abstract IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
}