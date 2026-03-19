using System.Linq.Expressions;
using Aero.Core.Data;
using Aero.Core.Railway;
using Marten;

namespace Aero.Models.Entities;

public interface IUserProfileRepository : IGenericRepository<AeroUserProfile, ulong>
{
    IDocumentSession session { get; }
    /// <summary>
    /// Gets only the user's profile.
    /// </summary>
    Task<Option<AeroUserProfile>> GetUserProfileAsync(ulong userId);
    Task SaveUserProfileAsync(AeroUserProfile user);
    Task DeleteUserProfileAsync(ulong userId);
}

