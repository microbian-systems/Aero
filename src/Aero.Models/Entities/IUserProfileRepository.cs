using Aero.Core.Data;
using Aero.Core.Railway;

namespace Aero.Models.Entities;

/// <summary>
/// Defines an interface for IUserProfileRepository.
/// </summary>
public interface IUserProfileRepository : IGenericRepository<AeroUserProfile, long>
{
    /// <summary>
    /// Gets only the user's profile.
    /// </summary>
    Task<Option<AeroUserProfile>> GetUserProfileAsync(long userId);
        /// <summary>
    /// SaveUserProfileAsync method.
    /// </summary>
Task SaveUserProfileAsync(AeroUserProfile user);
        /// <summary>
    /// DeleteUserProfileAsync method.
    /// </summary>
Task DeleteUserProfileAsync(long userId);
}

