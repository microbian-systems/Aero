using Aero.Core.Data;
using Aero.Core.Railway;

namespace Aero.Models.Entities;

public interface IUserProfileRepository : IGenericRepository<AeroUserProfile, long>
{
    /// <summary>
    /// Gets only the user's profile.
    /// </summary>
    Task<Option<AeroUserProfile>> GetUserProfileAsync(long userId);
    Task SaveUserProfileAsync(AeroUserProfile user);
    Task DeleteUserProfileAsync(long userId);
}

