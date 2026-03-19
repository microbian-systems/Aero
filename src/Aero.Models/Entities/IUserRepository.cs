using System.Threading.Tasks;
using Aero.Core.Data;
using Aero.Core.Railway;

namespace Aero.Models.Entities;

/// <summary>
/// Defines the repository interface for User-related operations.
/// </summary>
public interface IUserRepository : IGenericRepository<AeroUser, ulong>
{
    /// <summary>
    /// Gets a user with all related data (Profile, Settings).
    /// </summary>
    Task<Option<AeroUser>> GetFullUserById(ulong userId);

    /// <summary>
    /// Gets only the user's profile.
    /// </summary>
    Task<Option<AeroUserProfile>> GetUserProfileAsync(ulong userId);

    /// <summary>
    /// Gets only the user's settings.
    /// </summary>
    Task<Option<AeroUserSettings>> GetUserSettingsAsync(ulong userId);
}
