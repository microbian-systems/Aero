using System.Linq.Expressions;
using Aero.Core.Data;
using Aero.Core.Railway;
using Marten;

namespace Aero.Models.Entities;

public interface IUserProfileRepository : IGenericRepository<AeroUserProfile, long>
{
    IDocumentSession session { get; }
    /// <summary>
    /// Gets only the user's profile.
    /// </summary>
    Task<Option<AeroUserProfile>> GetUserProfileAsync(long userId);
    Task SaveUserProfileAsync(AeroUserProfile user);
    Task DeleteUserProfileAsync(long userId);
}

