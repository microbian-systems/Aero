using System.Threading;
using Aero.Core.Data;
using Aero.Core.Entities;
using Aero.Core.Extensions;
using Microsoft.Extensions.Logging;


namespace Aero.Marten;

public interface IGenericMartenRepository<T, TKey> 
    : IGenericRepository<T, TKey>
    where T : IEntity<TKey>, new() 
    where TKey : IEquatable<TKey>
{
    IDocumentSession session { get; }
    Task SaveChangesAsync();
}

public interface IGenericMartenRepository<T> : IGenericRepository<T, ulong> where T : IEntity<ulong>, new();

public abstract class GenericMartenRepository<T>(IDocumentSession session, ILogger<GenericMartenRepository<T>> log)
    : GenericMartenRepository<T, ulong>(session, log), IGenericMartenRepository<T>
    where T : ISnowflakeEntity, new();

public class GenericMartenRepository<T, TKey>(IDocumentSession session, ILogger<GenericMartenRepository<T, TKey>> log) 
    : GenericRepository<T, TKey>(log), IGenericMartenRepository<T, TKey>
    where T : IEntity<TKey>, new() where TKey : IEquatable<TKey>
{
    public IDocumentSession session { get; } = session;

    public override async Task<long> CountAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public override async Task<bool> ExistsAsync(TKey id)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public override async Task<IEnumerable<T>> GetAllAsync() =>
        await session.Query<T>().ToListAsync(CancellationToken.None);

    public override async Task<T> GetByIdAsync(TKey id)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public override async Task<IReadOnlyCollection<T>> GetByIdsAsync(IEnumerable<TKey> ids)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public override IEnumerable<T> Find(Expression<Func<T, bool>> strategy) =>
        FindAsync(strategy).GetAwaiter().GetResult();

    public override async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        log.LogInformation($"querying marten store...");
        var results = await session.Query<T>()
            .Where(predicate).ToListAsync();
        return results;
    }

    public override async Task<T> FindByIdAsync(TKey id)
    {
        log.LogInformation($"search for entity with id {id}");
        var result = await session.Query<T>()
            .Where(x => x.Id.Equals(id)).SingleAsync(); // todo - verifies this .Equals() method owrks
        return result;
    }

    public override async Task<T> InsertAsync(T entity)
    {
        await Task.CompletedTask;
        log.LogInformation($"inserting entity {entity.Dump()}");
        session.Store(entity);
        return entity;
    }

    public override async Task<T> UpdateAsync(T entity)
    {
        log.LogInformation($"updating entity {entity.Dump()}");
        session.Store(entity);
        await session.SaveChangesAsync();
        return entity;
    }

    public override async Task<T> UpsertAsync(T entity)
    {
        log.LogInformation($"upserting entity {entity.Dump()}");
        session.Store(entity);
        await session.SaveChangesAsync();
        return entity;
    }

    public override async Task DeleteAsync(TKey id)
    {
        log.LogInformation($"deleting entity with id {id}");
        session.Delete(id);
    }

    public override async Task DeleteAsync(T entity) => DeleteAsync(entity.Id).GetAwaiter().GetResult();

    public async Task SaveChangesAsync() => await session.SaveChangesAsync();
}