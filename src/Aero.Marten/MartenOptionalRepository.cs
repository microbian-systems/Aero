
namespace Aero.Marten;

public interface IMartenReadonlyRepositorySyncOption<T, TKey> 
    : IReadonlyRepositorySyncOption<T,TKey> 
    where TKey : IEquatable<TKey>
    where T : IEntity<TKey>;

public interface IMartenReadonlyRepositoryAsyncOption<T, TKey>
    : IReadonlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey>
    where TKey : IEquatable<TKey>;

public interface IMartenReadOnlyRepositoryOption<T, TKey>
    : IMartenReadonlyRepositorySyncOption<T, TKey>, IMartenReadonlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey> 
    where TKey : IEquatable<TKey>;

public interface IMartenWriteOnlyRepositorySyncOption<T, TKey> 
    : IWriteOnlyRepositorySyncOption<T, TKey>
    where T : IEntity<TKey>
    where TKey : IEquatable<TKey>;


public interface IMartenWriteOnlyRepositoryAsyncOption<T, TKey> 
    : IWriteOnlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey> 
    where TKey : IEquatable<TKey>;

public interface IMartenWriteOnlyRepositoryOption<T, TKey>
    : IMartenWriteOnlyRepositorySyncOption<T, TKey>, IMartenWriteOnlyRepositoryAsyncOption<T, TKey>
    where T : IEntity<TKey> 
    where TKey : IEquatable<TKey>;

public interface IMartenGenericRepositoryOption<T, TKey>
    : IMartenReadOnlyRepositoryOption<T, TKey>, IMartenWriteOnlyRepositoryOption<T, TKey>
    where T : IEntity<TKey>, new() where TKey : IEquatable<TKey>;

/// <summary>
/// The main Generic repository for interface for implementing generic repositories.
/// This is for the main database used by the application the majority of the time. If
/// any specific repository is needed, don't swap the DI registration for this. Create a new
/// DI registration for the specific interface & concrete implementation.
/// </summary>
/// <typeparam name="T">The type of data model to be operated upon <see cref="IEntity{TKey}"/></typeparam>
/// <remarks>long is the default type for the primary key due to the Aero use of the snowflake algorithm</remarks>
public interface IMartenGenericRepositoryOption<T> : IMartenGenericRepositoryOption<T, long> where T : IEntity<long>, new();


public class MartenGenericRepositoryOption<T>(IDocumentSession session, ILogger<MartenGenericRepositoryOption<T>> log)
    : MartenGenericRepositoryOption<T, long>(session, log), IMartenGenericRepositoryOption<T>
    where T : IEntity<long>, new()
{
    public override async Task<long> CountAsync(CancellationToken ct = default)
    {
        var count = await session.Query<T>()
            .LongCountAsync(ct);
        return count;
    }

    public override async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        session.Delete<T>(id);
        var result = await session.SaveChangesAsync(ct)
            .ContinueWith(t => t.IsCompletedSuccessfully, ct);
        return result;
    }

    public override async Task<bool> DeleteAsync(T entity, CancellationToken ct = default)
        => await DeleteAsync(entity.Id, ct);

    public override async Task<bool> ExistsAsync(long id, CancellationToken ct = default)
    {
        var exists = await session.Query<T>()
            .AnyAsync(e => e.Id == id, token: ct);
        return exists;
    }


    // todo - use paging w/ find (add to base interface):
    public override async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var results = await session.Query<T>()
            .Where(predicate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return results ?? [];
    }

    public override async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        var p = predicate.Compile();
        var results = await session.Query<T>()
            .Where(predicate)
            .ToListAsync(ct);
        return results ?? [];
    }

    public override async Task<Option<T>> FindByIdAsync(long id, CancellationToken ct = default)
    {
        var res = await session.LoadAsync<T>(id, ct);
        
        return res is not null
            ? Some(res)
            : None;
    }

    public override async Task<IEnumerable<T>> GetAllAsync(int page = 1, int num = 10, CancellationToken ct = default)
    {
        if(page < 1) { 
            page = 1; 
            log.LogWarning("Page number must be greater than 0. Defaulting to page 1.");
        }
        var records = await session.Query<T>()
            .Skip((page - 1) * num)
            .Take(num)
            .ToListAsync(ct);
        return records?.AsEnumerable() ?? [];
    }

    public override async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default)
    {
        var entities = await session.LoadManyAsync<T>(ct, ids);

        return entities ?? [];
    }

    public override async Task<T> InsertAsync(T entity, CancellationToken ct = default)
    {
        session.Store<T>(entity);
        await session.SaveChangesAsync(ct);

        return entity;
    }

    public override async Task<T> UpdateAsync(T entity, CancellationToken ct = default)
    {
        session.Store<T>(entity);
        await session.SaveChangesAsync(ct);

        return entity;
    }

    public override async Task<T> UpsertAsync(T entity, CancellationToken ct = default)
    {
        var id = entity.Id;
        var exists = session.LoadAsync<T>(id);

        return exists switch
        {
            not null => await UpdateAsync(entity, ct),
            _ => await InsertAsync(entity, ct)
        };
    }
}

public abstract class MartenGenericRepositoryOption<T, TKey>(IDocumentSession session, ILogger<MartenGenericRepositoryOption<T, TKey>> log)
    : GenericRepositoryOption<T, TKey>(log), IMartenGenericRepositoryOption<T, TKey>
    where T : IEntity<TKey>, new()
    where TKey : IEquatable<TKey>;
