using System.Security.Claims;

namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for ClaimsPrincipalExtensions.
/// </summary>
public static class ClaimsPrincipalExtensions
{
        /// <summary>
    /// GetFirstName method.
    /// </summary>
public static string GetFirstName(this ClaimsPrincipal claimsPrincipal)
        => claimsPrincipal.FindFirstValue(ClaimTypes.Name);

        /// <summary>
    /// GetLastName method.
    /// </summary>
public static string GetLastName(this ClaimsPrincipal claimsPrincipal)
        => claimsPrincipal.FindFirstValue(ClaimTypes.Surname);

        /// <summary>
    /// GetPhoneNumber method.
    /// </summary>
public static string GetPhoneNumber(this ClaimsPrincipal claimsPrincipal)
        => claimsPrincipal.FindFirstValue(ClaimTypes.MobilePhone);

    // public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
    //    => claimsPrincipal.FindFirstValue("id");
        /// <summary>
    /// GetUserId method.
    /// </summary>
public static string GetUserId(this ClaimsPrincipal principal)
        => GetUserId<string>(principal);

        /// <summary>
    /// GetUserId method.
    /// </summary>
public static T GetUserId<T>(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (typeof(T) == typeof(string))
        {
            return (T)Convert.ChangeType(userId, typeof(T));
        }
        else if (typeof(T) == typeof(int) || typeof(T) == typeof(long))
        {
            return userId != null ? (T)Convert.ChangeType(userId, typeof(T)) : (T)Convert.ChangeType(0, typeof(T));
        }
        else if (typeof(T) == typeof(Guid))
        {
            return userId != null ? (T)Convert.ChangeType(userId, typeof(T)) : (T)Convert.ChangeType(0, typeof(T));
        }
        else
        {
            throw new Exception("Invalid type provided");
        }
    }

        /// <summary>
    /// GetUserName method.
    /// </summary>
public static string GetUserName(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.FindFirstValue(ClaimTypes.Name);
    }

        /// <summary>
    /// GetUserEmail method.
    /// </summary>
public static string GetUserEmail(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.FindFirstValue(ClaimTypes.Email);
    }
}