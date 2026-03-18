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
    Task<T> AddAsync(T entity);
    Task AddAsync(IEnumerable<T> entities);
    Task<long> RemoveAllAsync();
    Task RemoveAsync(IEnumerable<TKey> ids);
    Task RemoveAsync(TKey id);
    Task RemoveAsync(T entity);
    Task RemoveAsync(IEnumerable<T> entities);
    Task SaveAsync(IEnumerable<T> entities);
    Task<T> SaveAsync(T entity);
}

public interface IRepository<T, TKey> : IReadOnlyRepository<T, TKey>, IWriteRepository<T, TKey>
    where T : IEntity<TKey>
    where TKey : IEquatable<TKey>
{
}