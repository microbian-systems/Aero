using Aero.Core.Entities;

namespace Aero.Core.Data;



/// <summary>
/// Defines a write-only repository for a given entity type.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
public interface IWriteRepository<T, TKey>
    where T : IEntity<TKey> 
    where TKey : IEquatable<TKey>
{
        /// <summary>
    /// AddAsync method.
    /// </summary>
Task<T> AddAsync(T entity);
        /// <summary>
    /// AddAsync method.
    /// </summary>
Task AddAsync(IEnumerable<T> entities);
        /// <summary>
    /// RemoveAllAsync method.
    /// </summary>
Task<long> RemoveAllAsync();
        /// <summary>
    /// RemoveAsync method.
    /// </summary>
Task RemoveAsync(IEnumerable<TKey> ids);
        /// <summary>
    /// RemoveAsync method.
    /// </summary>
Task RemoveAsync(TKey id);
        /// <summary>
    /// RemoveAsync method.
    /// </summary>
Task RemoveAsync(T entity);
        /// <summary>
    /// RemoveAsync method.
    /// </summary>
Task RemoveAsync(IEnumerable<T> entities);
        /// <summary>
    /// SaveAsync method.
    /// </summary>
Task SaveAsync(IEnumerable<T> entities);
        /// <summary>
    /// SaveAsync method.
    /// </summary>
Task<T> SaveAsync(T entity);
}

/// <summary>
/// Defines an interface for IRepository.
/// </summary>
public interface IRepository<T, TKey> : IReadOnlyRepository<T, TKey>, IWriteRepository<T, TKey>
    where T : IEntity<TKey>
    where TKey : IEquatable<TKey>
{
}