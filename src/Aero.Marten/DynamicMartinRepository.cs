using System.Collections.ObjectModel;

namespace Aero.Marten;

// todo - consider placing a constraint on type TKey for the marten repositories
/// <summary>
/// Defines an interface for IDynamicMartenRepository.
/// </summary>
public interface IDynamicMartenRepository : IDynamicRepositoryAsync<long>
{
}

/// <summary>
/// Represents a class for DynamicMartinRepository.
/// </summary>
public class DynamicMartinRepository(IDocumentSession db, ILogger<DynamicMartinRepository> log)
    : IDynamicMartenRepository
{
    private readonly ILogger<DynamicMartinRepository> log = log;

    // todo - implement InvalidateCache for marten repo
        /// <summary>
    /// InvalidateCacheAsync method.
    /// </summary>
public async Task InvalidateCacheAsync<T>(IEnumerable<T> documents) where T : class, IEntity<long>, new()
    {
        throw new NotImplementedException();
    }

        /// <summary>
    /// CountAsync method.
    /// </summary>
public async Task<long> CountAsync<T>() where T : class, IEntity<long>, new()
    {
        return await db.Query<T>().CountAsync(CancellationToken.None)
            .ConfigureAwait(false);
    }

        /// <summary>
    /// GetByIdAsync method.
    /// </summary>
public async Task<T> GetByIdAsync<T>(long id) where T : class, IEntity<long>, new()
    {
        return await db.Query<T>().FirstAsync(x => Equals(x.Id, id)).ConfigureAwait(false);
    }

        /// <summary>
    /// GetByIdsAsync method.
    /// </summary>
public async Task<IReadOnlyCollection<T>> GetByIdsAsync<T>(List<long> ids) where T : class, IEntity<long>, new()
    {
        var batch = db.CreateBatchQuery();
        var res = await batch.LoadMany<T>().ByIdList(ids);
        await batch.Execute();

        return new ReadOnlyCollection<T>(res.ToArray());
    }

        /// <summary>
    /// FindSingle method.
    /// </summary>
public async Task<T> FindSingle<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity<long>, new()
    {
        return await db.Query<T>().FirstOrDefaultAsync<T>(predicate);
    }

    Expression<Func<T, bool>> FuncToExpression<T>(Func<T, bool> func)
    {
        return x => func(x);
    }

        /// <summary>
    /// Search method.
    /// </summary>
public async Task<IEnumerable<T>> Search<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity<long>, new()
    {
        var results = (await db.Query<T>().Where(predicate)
                .ToListAsync())
            .AsEnumerable();

        return results;
    }

    // todo - add FindAllAsync(Func<> predicate) or Where(Func<> predicate)
        /// <summary>
    /// GetAllAsync method.
    /// </summary>
public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class, IEntity<long>, new()
    {
        return await db.Query<T>().ToListAsync(CancellationToken.None);
    }

        /// <summary>
    /// ExistsAsync method.
    /// </summary>
public async Task<bool> ExistsAsync<T>(long id) where T : class, IEntity<long>, new()
    {
        var res = await db.Query<T>().FirstAsync(x => Equals(x.Id, id));
        return res != null;
    }

        /// <summary>
    /// AddAsync method.
    /// </summary>
protected async Task<T> AddAsync<T>(T document) where T : class, IEntity<long>, new()
    {
        db.Store(document);
        await db.SaveChangesAsync();
        return document; // todo - verify martne reutrns a new id after saving
    }

        /// <summary>
    /// AddAsync method.
    /// </summary>
protected async Task AddAsync<T>(IEnumerable<T> documents) where T : class, IEntity<long>, new()
    {
        db.Store(documents);
        await db.SaveChangesAsync();
    }

        /// <summary>
    /// SaveAsync method.
    /// </summary>
public async Task<T> SaveAsync<T>(T document) where T : class, IEntity<long>, new()
    {
        var res = await AddAsync(document);
        return res;
    }

        /// <summary>
    /// SaveAsync method.
    /// </summary>
public async Task SaveAsync<T>(IEnumerable<T> documents) where T : class, IEntity<long>, new()
    {
        await AddAsync(documents);
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task DeleteAsync<T>(long id) where T : class, IEntity<long>, new()
    {
        db.Delete<T>(id);
        await db.SaveChangesAsync();
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task DeleteAsync<T>(List<long> ids) where T : class, IEntity<long>, new()
    {
        // todo - fix batch deletes for marten
        foreach (var id in ids)
            db.Delete<T>(id);
        await db.SaveChangesAsync();
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task DeleteAsync<T>(T document) where T : class, IEntity<long>, new()
    {
        db.Delete<T>(document);
        await db.SaveChangesAsync();
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task DeleteAsync<T>(IEnumerable<T> documents) where T : class, IEntity<long>, new()
    {
        // todo - fix batch deleteds for marten
        foreach (var doc in documents)
            db.Delete<T>(doc);
        await db.SaveChangesAsync();
    }
}