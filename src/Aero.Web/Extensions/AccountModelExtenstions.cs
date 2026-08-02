using Aero.Models.Entities;

namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for AccountModelExtensions.
/// </summary>
public static class AccountModelExtensions
{
        /// <summary>
    /// IsRefreshTokenValid method.
    /// </summary>
public static bool IsRefreshTokenValid(this ApiAccountModel model, string refreshToken)
    {
        if (model.RefreshToken != refreshToken || model.RefreshTokenExpiry <= DateTime.Now)
        {
            return false;
        }

        return true;
    }

        /// <summary>
    /// IsRefreshDateValid method.
    /// </summary>
public static bool IsRefreshDateValid(this ApiAccountModel model)
        => model.RefreshTokenExpiry >= DateTimeOffset.UtcNow;
}