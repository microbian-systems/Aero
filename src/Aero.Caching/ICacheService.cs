

using Aero.Core.Railway;

namespace Aero.Caching;

/// <summary>
/// A generic caching interface for a variety of caching operations.
/// It supports basic GET/SET, as well as Redis-specific features like hashes and atomic counters.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>An <see cref="Option{T}"/> containing the value if found.</returns>
    Task<Option<T>> GetAsync<T>(string key);

    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>An <see cref="Option{T}"/> containing the value if found.</returns>
    Option<T> Get<T>(string key);
    /// <summary>
    /// Gets a cached value or sets it using the provided factory if missing (asynchronously).
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value.</param>
    /// <param name="absoluteExpiration">Optional absolute expiration timespan.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<Option<T>> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null);

    /// <summary>
    /// Gets a cached value or sets it using the provided factory if missing.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value.</param>
    /// <param name="absoluteExpiration">Optional absolute expiration timespan.</param>
    /// <returns>The cached or newly created value.</returns>
    Option<T> GetOrSet<T>(string key, Func<T> factory, TimeSpan? absoluteExpiration = null);
    /// <summary>
    /// Sets a value in the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="absoluteExpiration">Optional absolute expiration timespan.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="absoluteExpiration">Optional absolute expiration timespan.</param>
    void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null);
    /// <summary>
    /// Sets an enumerable collection of values in the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The collection of values to cache.</param>
    /// <param name="absoluteExpiration">Optional absolute expiration timespan.</param>
    Task SetAsync<T>(string key, IEnumerable<T> value, TimeSpan? absoluteExpiration = null);

    /// <summary>
    /// Sets an enumerable collection of values in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The collection of values to cache.</param>
    /// <param name="absoluteExpiration">Optional absolute expiration timespan.</param>
    void Set<T>(string key, IEnumerable<T> value, TimeSpan? absoluteExpiration = null);

    /// <summary>
    /// Deletes a cached value by key asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    Task DeleteAsync(string key);

    /// <summary>
    /// Deletes a cached value by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    void Delete(string key);

    /// <summary>
    /// Checks if a cache key exists asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    Task<bool> KeyExistsAsync(string key);

    /// <summary>
    /// Checks if a cache key exists.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    bool KeyExists(string key);
    /// <summary>
    /// Increments a cached value asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The amount to increment by.</param>
    /// <returns>The new value.</returns>
    Task<long> IncrementAsync(string key, long value = 1);
    /// <summary>
    /// Increments a cached value.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The amount to increment by.</param>
    /// <returns>The new value after incrementing.</returns>
    long Increment(string key, long value = 1);

    /// <summary>
    /// Decrements a cached value asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The amount to decrement by.</param>
    /// <returns>The new value after decrementing.</returns>
    Task<long> DecrementAsync(string key, long value = 1);

    /// <summary>
    /// Decrements a cached value.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The amount to decrement by.</param>
    /// <returns>The new value after decrementing.</returns>
    long Decrement(string key, long value = 1);
    /// <summary>
    /// Sets a field in a cached hash set asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="field">The field name within the hash.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>True if setting was successful; otherwise, false.</returns>
    Task<bool> HashSetAsync<T>(string key, string field, T value);
    /// <summary>
    /// Sets a field in a cached hash set.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="field">The field name within the hash.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>True if setting was successful; otherwise, false.</returns>
    bool HashSet<T>(string key, string field, T value);
    /// <summary>
    /// Gets a field from a cached hash set asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="field">The field name.</param>
    /// <returns>An <see cref="Option{T}"/> containing the value if found.</returns>
    Task<Option<T>> HashGetAsync<T>(string key, string field);
    /// <summary>
    /// Gets a field from a cached hash set.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="field">The field name.</param>
    /// <returns>An <see cref="Option{T}"/> containing the value if found.</returns>
    Option<T> HashGet<T>(string key, string field);
    /// <summary>
    /// Gets all fields from a cached hash set asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>An <see cref="Option{T}"/> containing a dictionary of all fields if found.</returns>
    Task<Option<Dictionary<string, T>>> HashGetAllAsync<T>(string key);
    /// <summary>
    /// Gets all fields from a cached hash set.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>An <see cref="Option{T}"/> containing a dictionary of all fields if found.</returns>
    Option<Dictionary<string, T>> HashGetAll<T>(string key);
}