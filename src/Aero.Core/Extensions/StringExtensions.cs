using System.Globalization;
using System.Text.RegularExpressions;

namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for StringExtensions.
/// </summary>
public static class StringExtensions
{
        /// <summary>
    /// FromByteArray method.
    /// </summary>
public static string FromByteArray(this byte[] bytes)
        => Encoding.ASCII.GetString(bytes);
    
        /// <summary>
    /// ToByteArray method.
    /// </summary>
public static byte[] ToByteArray(this string str)
        => Encoding.ASCII.GetBytes(str);
        /// <summary>
    /// ToBase64 method.
    /// </summary>
public static string ToBase64(this string str) 
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    
        /// <summary>
    /// FromBase64 method.
    /// </summary>
public static string FromBase64(this string str) 
        => Encoding.UTF8.GetString(Convert.FromBase64String(str));
    
        /// <summary>
    /// ToBase64 method.
    /// </summary>
public static string ToBase64(this byte[] bytes) 
        => Convert.ToBase64String(bytes);
    
        /// <summary>
    /// FromBase64ToBytes method.
    /// </summary>
public static byte[] FromBase64ToBytes(this string str)
        => Convert.FromBase64String(str);
        /// <summary>
    /// IsNullOrEmpty method.
    /// </summary>
public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
        
        /// <summary>
    /// ToTitleCase method.
    /// </summary>
public static string ToTitleCase(this string word) =>
        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word.ToLower());
        
        /// <summary>
    /// IsValidEmail method.
    /// </summary>
public static bool IsValidEmail(this string email) => RegExMatch(email, RegExConstants.Email);

    private static bool RegExMatch(string val, string pattern) => Regex.Match(val, pattern).Success;
        
        /// <summary>
    /// ToCamelCase method.
    /// </summary>
public static string ToCamelCase(this string str)
    {
        var pattern = new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");
        return new string(
            new CultureInfo("en-US", false)
                .TextInfo
                .ToTitleCase(
                    string.Join(" ", pattern.Matches(str)).ToLower()
                )
                .Replace(@" ", "")
                .Select((x, i) => i == 0 ? char.ToLower(x) : x)
                .ToArray()
        );
    }
}