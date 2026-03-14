using System.Threading.Tasks;
using Aero.Core.Data;
using Aero.Core.Railway;

namespace Aero.Models.Entities;

/// <summary>
/// Defines the repository interface for User-related operations.
/// </summary>
public interface IUserRepository : IGenericRepository<AeroUser, string>
{
    /// <summary>
    /// Gets a user with all related data (Profile, Settings).
    /// </summary>
    Task<Option<AeroUser>> GetFullUserById(string userId);

    /// <summary>
    /// Gets only the user's profile.
    /// </summary>
    Task<Option<AeroUserProfile>> GetUserProfileAsync(string userId);

    /// <summary>
    /// Gets only the user's settings.
    /// </summary>
    Task<Option<UserSettingsModel>> GetUserSettingsAsync(string userId);
}
