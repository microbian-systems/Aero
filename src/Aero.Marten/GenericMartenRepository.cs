using Aero.Core.Data;
using Aero.Core.Extensions;


namespace Aero.Marten;

/// <summary>
/// Defines an interface for IGenericMartenRepository.
/// </summary>
public interface IGenericMartenRepository<T, TKey> 
    : IGenericRepository<T, TKey>
    where T : IEntity<TKey>, new() 
    where TKey : IEquatable<TKey>
{
        /// <summary>
    /// Gets or sets the session.
    /// </summary>
IDocumentSession session { get; }
        /// <summary>
    /// SaveChangesAsync method.
    /// </summary>
Task SaveChangesAsync();
}

/// <summary>
/// Defines an interface for IGenericMartenRepository.
/// </summary>
public interface IGenericMartenRepository<T> : IGenericRepository<T, long> where T : IEntity<long>, new();

/// <summary>
/// Represents a class for GenericMartenRepository.
/// </summary>
public abstract class GenericMartenRepository<T>(IDocumentSession session, ILogger<GenericMartenRepository<T>> log)
    : GenericMartenRepository<T, long>(session, log), IGenericMartenRepository<T>
    where T : ISnowflakeEntity, new();

/// <summary>
/// Represents a class for GenericMartenRepository.
/// </summary>
public class GenericMartenRepository<T, TKey>(IDocumentSession session, ILogger<GenericMartenRepository<T, TKey>> log) 
    : GenericRepository<T, TKey>(log), IGenericMartenRepository<T, TKey>
    where T : IEntity<TKey>, new() where TKey : IEquatable<TKey>
{
        /// <summary>
    /// Gets or sets the session.
    /// </summary>
public IDocumentSession session { get; } = session;

        /// <summary>
    /// CountAsync method.
    /// </summary>
public override async Task<long> CountAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

        /// <summary>
    /// ExistsAsync method.
    /// </summary>
public override async Task<bool> ExistsAsync(TKey id)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

        /// <summary>
    /// GetAllAsync method.
    /// </summary>
public override async Task<IEnumerable<T>> GetAllAsync() =>
        await session.Query<T>().ToListAsync(CancellationToken.None);

        /// <summary>
    /// GetByIdAsync method.
    /// </summary>
public override async Task<T> GetByIdAsync(TKey id)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

        /// <summary>
    /// GetByIdsAsync method.
    /// </summary>
public override async Task<IReadOnlyCollection<T>> GetByIdsAsync(IEnumerable<TKey> ids)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

        /// <summary>
    /// Find method.
    /// </summary>
public override IEnumerable<T> Find(Expression<Func<T, bool>> strategy) =>
        FindAsync(strategy).GetAwaiter().GetResult();

        /// <summary>
    /// FindAsync method.
    /// </summary>
public override async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        log.LogInformation($"querying marten store...");
        var results = await session.Query<T>()
            .Where(predicate).ToListAsync();
        return results;
    }

        /// <summary>
    /// FindByIdAsync method.
    /// </summary>
public override async Task<T> FindByIdAsync(TKey id)
    {
        log.LogInformation($"search for entity with id {id}");
        var result = await session.Query<T>()
            .Where(x => x.Id.Equals(id)).SingleAsync(); // todo - verifies this .Equals() method owrks
        return result;
    }

        /// <summary>
    /// InsertAsync method.
    /// </summary>
public override async Task<T> InsertAsync(T entity)
    {
        await Task.CompletedTask;
        log.LogInformation($"inserting entity {entity.Dump()}");
        session.Store(entity);
        return entity;
    }

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public override async Task<T> UpdateAsync(T entity)
    {
        log.LogInformation($"updating entity {entity.Dump()}");
        session.Store(entity);
        await session.SaveChangesAsync();
        return entity;
    }

        /// <summary>
    /// UpsertAsync method.
    /// </summary>
public override async Task<T> UpsertAsync(T entity)
    {
        log.LogInformation($"upserting entity {entity.Dump()}");
        session.Store(entity);
        await session.SaveChangesAsync();
        return entity;
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public override async Task DeleteAsync(TKey id)
    {
        log.LogInformation($"deleting entity with id {id}");
        session.Delete(id);
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public override async Task DeleteAsync(T entity) => DeleteAsync(entity.Id).GetAwaiter().GetResult();

        /// <summary>
    /// SaveChangesAsync method.
    /// </summary>
public async Task SaveChangesAsync() => await session.SaveChangesAsync();
}