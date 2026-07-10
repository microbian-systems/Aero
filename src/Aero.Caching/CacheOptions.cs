namespace Aero.Caching;

/// <summary>
/// Defines an interface for ICacheOptions.
/// </summary>
public interface ICacheOptions
{
        /// <summary>
    /// Gets or sets the Expiry.
    /// </summary>
TimeSpan Expiry { get; set; }
        /// <summary>
    /// Gets or sets the Expiry Type.
    /// </summary>
ExpirationType ExpiryType { get; set; }
}

/// <summary>
/// Defines an enumeration for ExpirationType.
/// </summary>
public enum ExpirationType
{
    Sliding,
    Absolute
}

/// <summary>
/// Represents a class for CacheOptions.
/// </summary>
public sealed class CacheOptions : ICacheOptions
{
        /// <summary>
    /// Gets or sets the Expiry Type.
    /// </summary>
public ExpirationType ExpiryType { get; set; } = ExpirationType.Absolute;
        /// <summary>
    /// Gets or sets the Expiry.
    /// </summary>
public TimeSpan Expiry { get; set; } = TimeSpan.FromMinutes(15);

        /// <summary>
    /// SetSlidingExpiration method.
    /// </summary>
public static CacheOptions SetSlidingExpiration(int minutes)
    {
        var opts = new CacheOptions
        {
            ExpiryType = ExpirationType.Sliding,
            Expiry = TimeSpan.FromMinutes(minutes)
        };
        return opts;
    }

        /// <summary>
    /// SetAbsoluteExpiration method.
    /// </summary>
public static CacheOptions SetAbsoluteExpiration(int minutes)
    {
        var opts = new CacheOptions
        {
            ExpiryType = ExpirationType.Absolute,
            Expiry = TimeSpan.FromMinutes(minutes)
        };
        return opts;
    }

        /// <summary>
    /// SetSlidingExpiration method.
    /// </summary>
public static CacheOptions SetSlidingExpiration(TimeSpan span)
    {
        var opts = new CacheOptions
        {
            ExpiryType = ExpirationType.Sliding,
            Expiry = span
        };
        return opts;
    }

        /// <summary>
    /// SetAbsoluteExpiration method.
    /// </summary>
public static CacheOptions SetAbsoluteExpiration(TimeSpan span)
    {
        var opts = new CacheOptions
        {
            ExpiryType = ExpirationType.Absolute,
            Expiry = span
        };
        return opts;
    }
}

/// <summary>
/// Represents a class for CacheOptionExtension.
/// </summary>
public static class CacheOptionExtension
{
        /// <summary>
    /// SetSlidingExpiration method.
    /// </summary>
public static void SetSlidingExpiration(this ICacheOptions opts, int minutes)
    {
        opts.ExpiryType = ExpirationType.Sliding;
        opts.Expiry = TimeSpan.FromMinutes(minutes);
    }

        /// <summary>
    /// SetAbsoluteExpiration method.
    /// </summary>
public static void SetAbsoluteExpiration(this ICacheOptions opts, int minutes)
    {
        opts.ExpiryType = ExpirationType.Absolute;
        opts.Expiry = TimeSpan.FromMinutes(minutes);
    }
}