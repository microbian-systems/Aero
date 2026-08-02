

using Aero.Core.Railway;

namespace Aero.Caching;

// todo - add Async methods for Delete and rename to Remove or Invalidate
/// <summary>
/// Represents a class for CacheServiceBase.
/// </summary>
public abstract class CacheServiceBase(ILogger<CacheServiceBase> log)
    : ICacheService
{
        /// <summary>
    /// Delete method.
    /// </summary>
public abstract void Delete(string key);
        /// <summary>
    /// Set method.
    /// </summary>
public abstract void Set<T>(string key, IEnumerable<T> value, TimeSpan? absoluteExpiration = null);
        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public abstract Task DeleteAsync(string key);
        /// <summary>
    /// Get method.
    /// </summary>
public abstract Option<T> Get<T>(string key);
        /// <summary>
    /// GetAsync method.
    /// </summary>
public abstract Task<Option<T>> GetAsync<T>(string key);
        /// <summary>
    /// GetOrSet method.
    /// </summary>
public abstract Option<T> GetOrSet<T>(string key, Func<T> factory, TimeSpan? absoluteExpiration = null);
        /// <summary>
    /// GetOrSetAsync method.
    /// </summary>
public abstract Task<Option<T>> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null);
        /// <summary>
    /// KeyExists method.
    /// </summary>
public abstract bool KeyExists(string key);
        /// <summary>
    /// KeyExistsAsync method.
    /// </summary>
public abstract Task<bool> KeyExistsAsync(string key);
        /// <summary>
    /// Set method.
    /// </summary>
public abstract void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null);
        /// <summary>
    /// SetAsync method.
    /// </summary>
public abstract Task SetAsync<T>(string key, IEnumerable<T> value, TimeSpan? absoluteExpiration = null);
        /// <summary>
    /// SetAsync method.
    /// </summary>
public abstract Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null);
        /// <summary>
    /// Decrement method.
    /// </summary>
public abstract long Decrement(string key, long value = 1);
        /// <summary>
    /// DecrementAsync method.
    /// </summary>
public abstract Task<long> DecrementAsync(string key, long value = 1);
        /// <summary>
    /// HashGet method.
    /// </summary>
public abstract Option<T> HashGet<T>(string key, string field);
        /// <summary>
    /// HashGetAsync method.
    /// </summary>
public abstract Task<Option<T>> HashGetAsync<T>(string key, string field);
        /// <summary>
    /// HashGetAll method.
    /// </summary>
public abstract Option<Dictionary<string, T>> HashGetAll<T>(string key);
        /// <summary>
    /// HashGetAllAsync method.
    /// </summary>
public abstract Task<Option<Dictionary<string, T>>> HashGetAllAsync<T>(string key);
        /// <summary>
    /// HashSet method.
    /// </summary>
public abstract bool HashSet<T>(string key, string field, T value);
        /// <summary>
    /// HashSetAsync method.
    /// </summary>
public abstract Task<bool> HashSetAsync<T>(string key, string field, T value);
        /// <summary>
    /// Increment method.
    /// </summary>
public abstract long Increment(string key, long value = 1);
        /// <summary>
    /// IncrementAsync method.
    /// </summary>
public abstract Task<long> IncrementAsync(string key, long value = 1);
}