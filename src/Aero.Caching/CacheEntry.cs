namespace Aero.Caching;

/// <summary>
/// Represents a record for CacheEntry.
/// </summary>
public record CacheEntry<T>
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
    /// Gets or sets the Options.
    /// </summary>
public CacheOptions Options { get; set; } = new();
}