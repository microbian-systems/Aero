using System.Linq.Expressions;
using Aero.Core.Entities;

namespace Aero.Core.Data;

/// <summary>
/// Defines synchronous read operations for a repository.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IReadonlyRepositorySync<T, TKey> where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Gets all entities from the repository.
    /// </summary>
    /// <returns>An enumerable collection of all entities.</returns>
    public IEnumerable<T> GetAll();

    /// <summary>
    /// Finds an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public T FindById(TKey id);

    /// <summary>
    /// Finds entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>An enumerable collection of entities that match the predicate.</returns>
    public IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
}

/// <summary>
/// Defines asynchronous read operations for a repository.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IReadonlyRepositoryAsync<T, TKey> where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Gets all entities from the repository asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of all entities.</returns>
    public Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Finds an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
    public Task<T> FindByIdAsync(TKey id);

    /// <summary>
    /// Finds entities that match the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of entities that match the predicate.</returns>
    public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}

/// <summary>
/// Combines synchronous and asynchronous read operations for a repository.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IReadOnlyRepository<T, TKey> : IReadonlyRepositorySync<T, TKey>, IReadonlyRepositoryAsync<T, TKey>
    where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
}

/// <summary>
/// Defines synchronous write operations for a repository.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IWriteOnlyRepositorySync<T, TKey> where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Inserts a new entity into the repository.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>The inserted entity.</returns>
    public T Insert(T entity);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity.</returns>
    public T Update(T entity);

    /// <summary>
    /// Upserts (inserts or updates) an entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to upsert.</param>
    /// <returns>The upserted entity.</returns>
    public T Upsert(T entity);

    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    public void Delete(TKey id);

    /// <summary>
    /// Deletes the specified entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public void Delete(T entity);
}

/// <summary>
/// Defines asynchronous write operations for a repository.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IWriteOnlyRepositoryAsync<T, TKey> where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Inserts a new entity into the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the inserted entity.</returns>
    public Task<T> InsertAsync(T entity);

    /// <summary>
    /// Updates an existing entity in the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity.</returns>
    public Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Upserts (inserts or updates) an entity in the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to upsert.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the upserted entity.</returns>
    public Task<T> UpsertAsync(T entity);

    /// <summary>
    /// Deletes an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task DeleteAsync(TKey id);

    /// <summary>
    /// Deletes the specified entity from the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task DeleteAsync(T entity);
}

/// <summary>
/// Combines synchronous and asynchronous write operations for a repository.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IWriteOnlyRepository<T, TKey> : IWriteOnlyRepositorySync<T, TKey>, IWriteOnlyRepositoryAsync<T, TKey>
    where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
}

/// <summary>
/// Represents a generic repository interface combining read and write operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IGenericRepository<T, TKey> : IReadOnlyRepository<T, TKey>, IWriteOnlyRepository<T, TKey>
    where T : IEntity<TKey>, new() where TKey : IEquatable<TKey>
{
}

/// <summary>
/// The main Generic repository interface for implementing generic repositories.
/// This is for the main database used by the application the majority of the time. If
/// any specific repository is needed, don't swap the DI registration for this. Create a new
/// DI registration for the specific interface &amp; concrete implementation.
/// </summary>
/// <typeparam name="T">The type of data model to be operated upon <see cref="IEntity{TKey}"/></typeparam>
/// <remarks>Guid is the default type for the primary key due to the Aero nature of using document stores</remarks>
public interface IGenericRepository<T> : IGenericRepository<T, long> where T : ISnowflakeEntity, new()
{
}

/// <summary>
/// A base implementation of the generic repository.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class GenericRepository<T>(ILogger<GenericRepository<T>> log)
    : GenericRepository<T, long>(log), IGenericRepository<T>
    where T : ISnowflakeEntity, new();

/// <summary>
/// A base implementation of the generic repository with a custom key type.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public abstract class GenericRepository<T, TKey>(ILogger log) : IGenericRepository<T, TKey>
    where T : IEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The logger instance.
    /// </summary>
    protected readonly ILogger log = log;

    /// <inheritdoc/>
    public IEnumerable<T> GetAll() => GetAllAsync().GetAwaiter().GetResult();

    /// <inheritdoc/>
    public abstract Task<long> CountAsync();

    /// <inheritdoc/>
    public abstract Task<bool> ExistsAsync(TKey id);

    /// <inheritdoc/>
    public abstract Task<IEnumerable<T>> GetAllAsync();

    /// <inheritdoc/>
    public abstract Task<T> GetByIdAsync(TKey id);

    /// <inheritdoc/>
    public abstract Task<IReadOnlyCollection<T>> GetByIdsAsync(IEnumerable<TKey> ids);

    /// <inheritdoc/>
    public virtual T FindById(TKey id) => FindByIdAsync(id).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate) =>
        FindAsync(predicate).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public abstract Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <inheritdoc/>
    public abstract Task<T> FindByIdAsync(TKey id);

    // todo - add overloaded method with IEnumerable<> parameter to all insert/update/delete method
    /// <inheritdoc/>
    public virtual T Insert(T entity) => InsertAsync(entity).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public virtual T Update(T entity) => UpdateAsync(entity).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public virtual T Upsert(T entity) => UpsertAsync(entity).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public virtual void Delete(TKey id) => DeleteAsync(id).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public virtual void Delete(T entity) => DeleteAsync(entity).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public abstract Task<T> InsertAsync(T entity);

    /// <inheritdoc/>
    public abstract Task<T> UpdateAsync(T entity);

    /// <inheritdoc/>
    public abstract Task<T> UpsertAsync(T entity);

    /// <inheritdoc/>
    public abstract Task DeleteAsync(TKey id);

    /// <inheritdoc/>
    public abstract Task DeleteAsync(T entity);
}