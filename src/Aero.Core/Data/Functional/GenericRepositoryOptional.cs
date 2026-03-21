using System.Linq.Expressions;
using Aero.Core.Entities;
using Aero.Core.Railway;
// todo - rename aero.core.data.functional namespace to aero.core.data.railway
namespace Aero.Core.Data.Functional;

public interface IReadonlyRepositorySyncOption<T, TKey> 
    where T : IEntity<TKey> 
    where TKey : IEquatable<TKey>
{
    public IEnumerable<T> GetAll();
    public Option<T> FindById(TKey id);
    public IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
}

public interface IReadonlyRepositoryAsyncOption<T, TKey> where T 
    : IEntity<TKey> where TKey : IEquatable<TKey>
{
    public Task<IEnumerable<T>> GetAllAsync();

    public Task<Option<T>> FindByIdAsync(TKey id);

    // read here: https://stackoverflow.com/questions/793571/why-would-you-use-expressionfunct-rather-than-funct
    public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}

public interface IReadOnlyRepositoryOption<T, TKey>
    : IReadonlyRepositorySyncOption<T, TKey>, IReadonlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey> where TKey : IEquatable<TKey>;

public interface IWriteOnlyRepositorySyncOption<T, TKey> where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
    public T Insert(T entity);
    public T Update(T entity);
    public T Upsert(T entity);
    public bool Delete(TKey id);
    public bool Delete(T entity);
}

public interface IWriteOnlyRepositoryAsyncOption<T, TKey> where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
    public Task<T> InsertAsync(T entity);
    public Task<T> UpdateAsync(T entity);
    public Task<T> UpsertAsync(T entity);
    public Task<bool> DeleteAsync(TKey id);
    public Task<bool> DeleteAsync(T entity);
}

public interface IWriteOnlyRepositoryOption<T, TKey>
    : IWriteOnlyRepositorySyncOption<T, TKey>, IWriteOnlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey> where TKey : IEquatable<TKey>;

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

public abstract class GenericRepositoryOption<T>(ILogger<GenericRepositoryOption<T>> log)
    : GenericRepositoryOption<T, long>(log), IGenericRepositoryOption<T>
    where T : IEntity<long>, new();

public abstract class GenericRepositoryOption<T, TKey>(ILogger log) 
    : IGenericRepositoryOption<T, TKey>
    where T : IEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
    protected readonly ILogger log = log;

    public virtual IEnumerable<T> GetAll() => GetAllAsync().GetAwaiter().GetResult();

   public abstract Task<long> CountAsync();

    public abstract Task<bool> ExistsAsync(TKey id);

    public abstract Task<IEnumerable<T>> GetAllAsync();
    public abstract Task<Option<T>> GetByIdAsync(TKey id);

    public abstract Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<TKey> ids);

    public virtual Option<T> FindById(TKey id) => FindByIdAsync(id).GetAwaiter().GetResult();

    public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate) =>
        FindAsync(predicate).GetAwaiter().GetResult();

    public abstract Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    public abstract Task<Option<T>> FindByIdAsync(TKey id);

    // todo - add overloaded method with IEnumerable<> parameter to all insert/update/delete method
    public virtual T Insert(T entity) => InsertAsync(entity).GetAwaiter().GetResult();

    public virtual T Update(T entity) => UpdateAsync(entity).GetAwaiter().GetResult();

    public virtual T Upsert(T entity) => UpsertAsync(entity).GetAwaiter().GetResult();

    public virtual bool Delete(TKey id) => DeleteAsync(id).GetAwaiter().GetResult();

    public virtual bool Delete(T entity) => DeleteAsync(entity).GetAwaiter().GetResult();

    public abstract Task<T> InsertAsync(T entity);

    public abstract Task<T> UpdateAsync(T entity);

    public abstract Task<T> UpsertAsync(T entity);

    public abstract Task<bool> DeleteAsync(TKey id);

    public abstract Task<bool> DeleteAsync(T entity);
}