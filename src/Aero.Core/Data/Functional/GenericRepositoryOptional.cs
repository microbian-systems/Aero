using System.Linq.Expressions;
using Aero.Core.Entities;
using Aero.Core.Railway;
// todo - rename aero.core.data.functional namespace to aero.core.data.railway
namespace Aero.Core.Data.Functional;

/// <summary>
/// Defines an interface for IReadonlyRepositorySyncOption.
/// </summary>
public interface IReadonlyRepositorySyncOption<T, TKey> 
    where T : IEntity<TKey> 
    where TKey : IEquatable<TKey>
{
        /// <summary>
    /// GetAll method.
    /// </summary>
public IEnumerable<T> GetAll(int page=1, int num=10);
        /// <summary>
    /// FindById method.
    /// </summary>
public Option<T> FindById(TKey id);
        /// <summary>
    /// Find method.
    /// </summary>
public IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
}

/// <summary>
/// Defines an interface for IReadonlyRepositoryAsyncOption.
/// </summary>
public interface IReadonlyRepositoryAsyncOption<T, TKey> where T 
    : IEntity<TKey> where TKey : IEquatable<TKey>
{
        /// <summary>
    /// GetAllAsync method.
    /// </summary>
public Task<IEnumerable<T>> GetAllAsync(int page=1, int num=10, CancellationToken ct = default);

        /// <summary>
    /// FindByIdAsync method.
    /// </summary>
public Task<Option<T>> FindByIdAsync(TKey id, CancellationToken ct = default);

    // read here: https://stackoverflow.com/questions/793571/why-would-you-use-expressionfunct-rather-than-funct
        /// <summary>
    /// FindAsync method.
    /// </summary>
public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

        /// <summary>
    /// FindAsync method.
    /// </summary>
public Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default);
}

/// <summary>
/// Defines an interface for IReadOnlyRepositoryOption.
/// </summary>
public interface IReadOnlyRepositoryOption<T, TKey>
    : IReadonlyRepositorySyncOption<T, TKey>, IReadonlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey> where TKey : IEquatable<TKey>;

/// <summary>
/// Defines an interface for IWriteOnlyRepositorySyncOption.
/// </summary>
public interface IWriteOnlyRepositorySyncOption<T, TKey> where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
        /// <summary>
    /// Insert method.
    /// </summary>
public T Insert(T entity);
        /// <summary>
    /// Update method.
    /// </summary>
public T Update(T entity);
        /// <summary>
    /// Upsert method.
    /// </summary>
public T Upsert(T entity);
        /// <summary>
    /// Delete method.
    /// </summary>
public bool Delete(TKey id);
        /// <summary>
    /// Delete method.
    /// </summary>
public bool Delete(T entity);
}

/// <summary>
/// Defines an interface for IWriteOnlyRepositoryAsyncOption.
/// </summary>
public interface IWriteOnlyRepositoryAsyncOption<T, TKey> where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
        /// <summary>
    /// InsertAsync method.
    /// </summary>
public Task<T> InsertAsync(T entity, CancellationToken ct = default);
        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public Task<T> UpdateAsync(T entity, CancellationToken ct = default);
        /// <summary>
    /// UpsertAsync method.
    /// </summary>
public Task<T> UpsertAsync(T entity, CancellationToken ct = default);
        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public Task<bool> DeleteAsync(TKey id, CancellationToken ct = default);
        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public Task<bool> DeleteAsync(T entity, CancellationToken ct = default);
}

/// <summary>
/// Defines an interface for IWriteOnlyRepositoryOption.
/// </summary>
public interface IWriteOnlyRepositoryOption<T, TKey>
    : IWriteOnlyRepositorySyncOption<T, TKey>, IWriteOnlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey> where TKey : IEquatable<TKey>;

/// <summary>
/// Defines an interface for IGenericRepositoryOption.
/// </summary>
public interface IGenericRepositoryOption<T, TKey>
    : IReadOnlyRepositoryOption<T, TKey>, IWriteOnlyRepositoryOption<T, TKey>
    where T : IEntity<TKey>, new() where TKey : IEquatable<TKey>;

/// <summary>
/// The main Generic repository for interface for implementing generic repositories.
/// This is for the main database used by the application the majority of the time. If
/// any specific repository is needed, don't swap the DI registration for this. Create a new
/// DI registration for the specific interface & concrete implementation.
/// </summary>
/// <typeparam name="T">The type of data model to be operated upon <see cref="IEntity{TKey}"/></typeparam>
/// <remarks>long is the default type for the primary key due to the Aero use of the snowflake algorithm</remarks>
public interface IGenericRepositoryOption<T> : IGenericRepositoryOption<T, long> where T : IEntity<long>, new();

/// <summary>
/// Represents a class for GenericRepositoryOption.
/// </summary>
public abstract class GenericRepositoryOption<T>(ILogger<GenericRepositoryOption<T>> log)
    : GenericRepositoryOption<T, long>(log), IGenericRepositoryOption<T>
    where T : IEntity<long>, new();

/// <summary>
/// Represents a class for GenericRepositoryOption.
/// </summary>
public abstract class GenericRepositoryOption<T, TKey>(ILogger log) 
    : IGenericRepositoryOption<T, TKey>
    where T : IEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{

        /// <summary>
    /// GetAll method.
    /// </summary>
public virtual IEnumerable<T> GetAll(int page=1, int num=10) => GetAllAsync().GetAwaiter().GetResult();

      /// <summary>
   /// CountAsync method.
   /// </summary>
public abstract Task<long> CountAsync(CancellationToken ct = default);

        /// <summary>
    /// ExistsAsync method.
    /// </summary>
public abstract Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);

        /// <summary>
    /// GetAllAsync method.
    /// </summary>
public abstract Task<IEnumerable<T>> GetAllAsync(int page=1, int num=10, CancellationToken ct = default);

        /// <summary>
    /// GetByIdsAsync method.
    /// </summary>
public abstract Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken ct = default);

        /// <summary>
    /// FindById method.
    /// </summary>
public virtual Option<T> FindById(TKey id) => FindByIdAsync(id).GetAwaiter().GetResult();

        /// <summary>
    /// Find method.
    /// </summary>
public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate) =>
        FindAsync(predicate, default(CancellationToken)).GetAwaiter().GetResult();

        /// <summary>
    /// FindAsync method.
    /// </summary>
public abstract Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

        /// <summary>
    /// FindAsync method.
    /// </summary>
public abstract Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default);

        /// <summary>
    /// FindByIdAsync method.
    /// </summary>
public abstract Task<Option<T>> FindByIdAsync(TKey id, CancellationToken ct = default);

    // todo - add overloaded method with IEnumerable<> parameter to all insert/update/delete method
        /// <summary>
    /// Insert method.
    /// </summary>
public virtual T Insert(T entity) => InsertAsync(entity).GetAwaiter().GetResult();

        /// <summary>
    /// Update method.
    /// </summary>
public virtual T Update(T entity) => UpdateAsync(entity).GetAwaiter().GetResult();

        /// <summary>
    /// Upsert method.
    /// </summary>
public virtual T Upsert(T entity) => UpsertAsync(entity).GetAwaiter().GetResult();

        /// <summary>
    /// Delete method.
    /// </summary>
public virtual bool Delete(TKey id) => DeleteAsync(id).GetAwaiter().GetResult();

        /// <summary>
    /// Delete method.
    /// </summary>
public virtual bool Delete(T entity) => DeleteAsync(entity).GetAwaiter().GetResult();

        /// <summary>
    /// InsertAsync method.
    /// </summary>
public abstract Task<T> InsertAsync(T entity, CancellationToken ct = default);

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public abstract Task<T> UpdateAsync(T entity, CancellationToken ct = default);

        /// <summary>
    /// UpsertAsync method.
    /// </summary>
public abstract Task<T> UpsertAsync(T entity, CancellationToken ct = default);

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public abstract Task<bool> DeleteAsync(TKey id, CancellationToken ct = default);

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public abstract Task<bool> DeleteAsync(T entity, CancellationToken ct = default);
}