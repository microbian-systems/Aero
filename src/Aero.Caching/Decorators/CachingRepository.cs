using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Aero.Core.Data;
using Aero.Core.Entities;
using Aero.Core.Railway;

namespace Aero.Caching.Decorators;

/// <summary>
/// Represents a record for DbCacheResult.
/// </summary>
public abstract record DbCacheResult<T, TKey>
    where T : IEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
        /// <summary>
    /// Gets or sets the Key.
    /// </summary>
public string Key { get; set; }
        /// <summary>
    /// Gets or sets the Value.
    /// </summary>
public T Value { get; set; }
        /// <summary>
    /// Gets or sets the Success.
    /// </summary>
public bool Success { get; set; }
        /// <summary>
    /// Gets or sets the Timestamp.
    /// </summary>
public DateTime Timestamp { get; set; } = DateTime.UtcNow;    
}
/// <summary>
/// Represents a record for DbCacheResult.
/// </summary>
public sealed record DbCacheResult<T> : DbCacheResult<T, long> where T : ISnowflakeEntity, new() { }

// Todo - Consider not inheriting from IGenericRepository for the cache repository and change return values to DbCacheResult
/// <summary>
/// Defines an interface for ICachingRepositoryDecorator.
/// </summary>
public interface ICachingRepositoryDecorator<T, TKey> : IGenericRepository<T, TKey>
    where T : IEntity<TKey>, new() where TKey : IEquatable<TKey>
{
        /// <summary>
    /// Insert method.
    /// </summary>
T Insert(CacheEntry<T> entry);
        /// <summary>
    /// Update method.
    /// </summary>
T Update(CacheEntry<T> entry);
        /// <summary>
    /// Upsert method.
    /// </summary>
T Upsert(CacheEntry<T> entry);
        /// <summary>
    /// Insert method.
    /// </summary>
T Insert([NotNull] T entity, CacheOptions opts = default);
        /// <summary>
    /// Update method.
    /// </summary>
T Update([NotNull] T entity, CacheOptions opts = default);
        /// <summary>
    /// Upsert method.
    /// </summary>
T Upsert([NotNull] T entity, CacheOptions opts = default);

        /// <summary>
    /// InsertAsync method.
    /// </summary>
Task<T> InsertAsync(CacheEntry<T> entry);
        /// <summary>
    /// UpdateAsync method.
    /// </summary>
Task<T> UpdateAsync(CacheEntry<T> entry);
        /// <summary>
    /// UpsertAsync method.
    /// </summary>
Task<T> UpsertAsync(CacheEntry<T> entry);
        /// <summary>
    /// InsertAsync method.
    /// </summary>
Task<T> InsertAsync([NotNull] T entity, CacheOptions opts = default);
        /// <summary>
    /// UpdateAsync method.
    /// </summary>
Task<T> UpdateAsync([NotNull] T entity, CacheOptions opts = default);
        /// <summary>
    /// UpsertAsync method.
    /// </summary>
Task<T> UpsertAsync([NotNull] T entity, CacheOptions opts = default);
}

/// <summary>
/// Defines an interface for ICachingRepositoryDecorator.
/// </summary>
public interface ICachingRepositoryDecorator<T>
    : ICachingRepositoryDecorator<T, long>, IGenericRepository<T>
    where T : ISnowflakeEntity, new();


/// <summary>
/// Represents a class for CachingRepository.
/// </summary>
public class CachingRepository<T>(
    ICacheService cache,
    IGenericRepository<T, long> db,
    ILogger<CachingRepository<T, long>> log)
    : CachingRepository<T, long>(cache, db, log)
    where T : ISnowflakeEntity, new();

/// <summary>
/// Represents a class for CachingRepository.
/// </summary>
public class CachingRepository<T, TKey>(
    ICacheService cache,
    IGenericRepository<T, TKey> db,
    ILogger<CachingRepository<T, TKey>> log)
    : ICachingRepositoryDecorator<T, TKey>
    where T : IEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
        /// <summary>
    /// db.
    /// </summary>
protected readonly IGenericRepository<T,TKey> db = db;
        /// <summary>
    /// cache.
    /// </summary>
protected readonly ICacheService cache = cache;
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<CachingRepository<T, TKey>> log = log;
        /// <summary>
    /// type.
    /// </summary>
protected readonly string type = typeof(T).Name; // todo - make sure Type().Name is suffice for cache key
        /// <summary>
    /// defaultExpiration.
    /// </summary>
protected readonly TimeSpan defaultExpiration = TimeSpan.FromMinutes(15);
        /// <summary>
    /// prefix.
    /// </summary>
protected readonly string prefix = $"db_{typeof(T).Name}";  // todo - pull the cache-key prefix in from appSettings.json
        /// <summary>
    /// defaultOptions.
    /// </summary>
protected readonly CacheOptions defaultOptions = new();

        /// <summary>
    /// GetAll method.
    /// </summary>
public IEnumerable<T> GetAll() => GetAllAsync().GetAwaiter().GetResult();
    
        /// <summary>
    /// GetAllAsync method.
    /// </summary>
public async Task<IEnumerable<T>> GetAllAsync()
    {
        var key = $"{prefix}_all";
        var success = await cache.GetAsync<T>(key);
        
        if(success.IsNone)
        {
            log.LogInformation($"cache miss for {key}");
            var results = await db.GetAllAsync();
            
            if(results != null)
            {
                await cache.SetAsync(prefix, results, defaultOptions.Expiry);
                log.LogInformation($"cached {results.Count()} items");
                return results;
            }

            return [];
        }
        else
        {
            log.LogInformation($"cache hit for {key}");
            return success.AsEnumerable();
        }
    }
    
        /// <summary>
    /// FindById method.
    /// </summary>
public T FindById(TKey id) => FindByIdAsync(id).GetAwaiter().GetResult();

        /// <summary>
    /// FindByIdAsync method.
    /// </summary>
public async Task<T> FindByIdAsync(TKey id)
    {
        var key = $"{prefix}_{id}";
        var success = await cache.GetAsync<T>(key);

        var ret = success.Match(Some: x => x, None: () => default);
        
        if(success.IsNone)
        {
            log.LogInformation($"cache miss for {key}");
            var results = await db.FindByIdAsync(id);

            if(results != null)
                await cache.SetAsync(key, results);

            return ret;
        }
        else
        {
            log.LogInformation("cache hit for {key}", key);
            return ret;
        }
        
        log.LogInformation($"cache hit for {prefix}");
        
        return ret;
    }

        /// <summary>
    /// Find method.
    /// </summary>
public IEnumerable<T> Find(Expression<Func<T, bool>> predicate) => FindAsync(predicate).GetAwaiter().GetResult();

        /// <summary>
    /// FindAsync method.
    /// </summary>
public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        var key = $"{prefix}_find";
        var success = await cache.GetAsync<T>(key);
        
        if(success.IsNone)
        {
            log.LogInformation($"cache miss for {key}");
            var results = await db.FindAsync(predicate);
            if(results != null && results.Any())
            {
                await cache.SetAsync<T>(prefix, results);
                log.LogInformation($"cached {results.Count()} items");
                return results;
            }

            return [];
        }
        else
        {
            log.LogInformation("cache hit for {key}", key);
            return success.AsEnumerable();
        }
    }

        /// <summary>
    /// Insert method.
    /// </summary>
public T Insert(T entity) => InsertAsync(entity).GetAwaiter().GetResult();

        /// <summary>
    /// Update method.
    /// </summary>
public T Update(T entity) => UpdateAsync(entity).GetAwaiter().GetResult();

        /// <summary>
    /// Upsert method.
    /// </summary>
public T Upsert(T entity) => UpsertAsync(entity).GetAwaiter().GetResult();

        /// <summary>
    /// Delete method.
    /// </summary>
public void Delete(TKey id) => DeleteAsync(id).GetAwaiter().GetResult();

        /// <summary>
    /// Delete method.
    /// </summary>
public void Delete(T entity) => DeleteAsync(entity).GetAwaiter().GetResult();

        /// <summary>
    /// InsertAsync method.
    /// </summary>
public async Task<T> InsertAsync([NotNull] T entity)
    {
        var dbRes = await db.InsertAsync(entity);
        
        await cache.SetAsync(entity.Id.ToString(), entity, defaultExpiration);
        
        return dbRes;
    }

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public async Task<T> UpdateAsync([NotNull] T entity)
    {
        var dbRes = await db.UpdateAsync(entity);
        
        await cache.SetAsync(entity.Id.ToString(), entity, defaultExpiration);


        log.LogInformation($"cached item with id: {entity.Id}");
        
        return dbRes;
    }

        /// <summary>
    /// UpsertAsync method.
    /// </summary>
public async Task<T> UpsertAsync([NotNull] T entity)
    {
        var dbRes = await db.UpsertAsync(entity);
        
        await cache.SetAsync(entity.Id.ToString(), entity, defaultExpiration);

        log.LogInformation($"cached item with id: {entity.Id}");
        
        return dbRes;
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task DeleteAsync(TKey? id)
    {
        log.LogInformation($"removing item with id: {id} from cache");
        if (id is null)
        {
            log.LogWarning("they key was null, nothing to cache");
            return;
        }

        await cache.DeleteAsync(id?.ToString());

        await db.DeleteAsync(id);
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task DeleteAsync([NotNull] T entity) => await DeleteAsync(entity.Id);

        /// <summary>
    /// Insert method.
    /// </summary>
public T Insert(CacheEntry<T> entry) => InsertAsync(entry).GetAwaiter().GetResult();

        /// <summary>
    /// Update method.
    /// </summary>
public T Update(CacheEntry<T> entry) => UpdateAsync(entry).GetAwaiter().GetResult();

        /// <summary>
    /// Upsert method.
    /// </summary>
public T Upsert(CacheEntry<T> entry) => UpsertAsync(entry).GetAwaiter().GetResult();

        /// <summary>
    /// Insert method.
    /// </summary>
public T Insert([NotNull] T entity, CacheOptions opts = default) =>
        InsertAsync(entity, opts).GetAwaiter().GetResult();

        /// <summary>
    /// Update method.
    /// </summary>
public T Update([NotNull] T entity, CacheOptions opts = default) =>
        UpdateAsync(entity, opts).GetAwaiter().GetResult();

        /// <summary>
    /// Upsert method.
    /// </summary>
public T Upsert([NotNull] T entity, CacheOptions opts = default) =>
        UpdateAsync(entity, opts).GetAwaiter().GetResult();

        /// <summary>
    /// InsertAsync method.
    /// </summary>
public async Task<T> InsertAsync(CacheEntry<T> entry)
    {
        var dbRes = await db.InsertAsync(entry.Value);
        
        if(dbRes != null)
        {
            await cache.SetAsync(entry.Key, entry.Value, entry.Options.Expiry);
            // if(!res)
            //     log.LogWarning($"(inserting to cache failed for id: {entry.Key}");
        }
        
        return dbRes;
    }

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public async Task<T> UpdateAsync(CacheEntry<T> entry)
    {
        var dbRes = await db.UpdateAsync(entry.Value);
        
        if(dbRes != null)
        {
            await cache.SetAsync(entry.Key, entry.Value, entry.Options.Expiry);
        }
        
        return dbRes;
    }

        /// <summary>
    /// UpsertAsync method.
    /// </summary>
public async Task<T> UpsertAsync(CacheEntry<T> entry)
    {
        var dbRes = await db.UpsertAsync(entry.Value);
        
        if(dbRes != null)
        {
            await cache.SetAsync(entry.Key, entry.Value, entry.Options.Expiry);
        }
        
        return dbRes;
    }

        /// <summary>
    /// InsertAsync method.
    /// </summary>
public async Task<T> InsertAsync([NotNull] T entity, CacheOptions opts = default)
        => await InsertAsync(new CacheEntry<T>() { Key = entity.Id.ToString(), Value = entity});

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public async Task<T> UpdateAsync([NotNull] T entity, CacheOptions opts = default)
        => await UpdateAsync(new CacheEntry<T>() { Key = entity.Id.ToString(), Value = entity});


        /// <summary>
    /// UpsertAsync method.
    /// </summary>
public async Task<T> UpsertAsync([NotNull] T entity, CacheOptions opts = default)
        => await UpsertAsync(new CacheEntry<T>() { Key = entity.Id.ToString(), Value = entity});

}