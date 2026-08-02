namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for Base64Extensions.
/// </summary>
public static class Base64Extensions
{
        /// <summary>
    /// Base64Decode method.
    /// </summary>
public static string Base64Decode(this string data)
        => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data));

        /// <summary>
    /// Base64Encode method.
    /// </summary>
public static string Base64Encode(this string data) 
        => Base64Encode(System.Text.Encoding.UTF8.GetBytes(data));
        
        /// <summary>
    /// Base64Encode method.
    /// </summary>
public static string Base64Encode(this byte[] data) => Convert.ToBase64String(data);
}