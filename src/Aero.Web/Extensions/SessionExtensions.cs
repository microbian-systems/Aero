namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for SessionExtensions.
/// </summary>
public static class SessionExtensions
{
        /// <summary>
    /// Set method.
    /// </summary>
public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

        /// <summary>
    /// Get method.
    /// </summary>
public static T Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}