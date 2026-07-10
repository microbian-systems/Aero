using System.Linq.Expressions;
using Aero.Core.Entities;

namespace Aero.Services;

/// <summary>
/// Defines an interface for IAeroUserProfileService.
/// </summary>
public interface IAeroUserProfileService : IUserProfileService<AeroUserProfile>{}

/// <summary>
/// Represents a class for AeroUserProfileService.
/// </summary>
public class AeroUserProfileService(IUserProfileRepository userRepo, ILogger<AeroUserProfileService> log)
    : AeroUserProfileService<AeroUserProfile>(userRepo, log), IAeroUserProfileService;

/// <summary>
/// Defines an interface for IUserProfileService.
/// </summary>
public interface IUserProfileService<T> where T : AeroUserProfile, IEntity
{
        /// <summary>
    /// GetById method.
    /// </summary>
Task<T> GetById(long id);
        /// <summary>
    /// GetByEmail method.
    /// </summary>
Task<T> GetByEmail(string email);
        /// <summary>
    /// InsertAsync method.
    /// </summary>
Task InsertAsync(T model);
        /// <summary>
    /// UpdateAsync method.
    /// </summary>
Task UpdateAsync(T model);
        /// <summary>
    /// UpsertAsync method.
    /// </summary>
Task UpsertAsync(T model);
        /// <summary>
    /// DeleteAsync method.
    /// </summary>
Task DeleteAsync(T model);
        /// <summary>
    /// DeleteAsync method.
    /// </summary>
Task DeleteAsync(long id);
        /// <summary>
    /// FindAsync method.
    /// </summary>
Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}
    
/// <summary>
/// Represents a class for AeroUserProfileService.
/// </summary>
public class AeroUserProfileService<T>(IUserProfileRepository db, ILogger<AeroUserProfileService<T>> log)
    : IUserProfileService<T>
    where T : AeroUserProfile, new()
{

        /// <summary>
    /// GetById method.
    /// </summary>
public async Task<T> GetById(long id)
    {
        var results = await db.FindByIdAsync(id);
        return (T)(object)results!;
    }

        /// <summary>
    /// GetByEmail method.
    /// </summary>
public async Task<T> GetByEmail(string email)
    {
        ThrowGuard.Throw.NotImplemented("have to figure out how to get the user profile with marten");
        return await Task.FromResult<T>(default!);
    }

        /// <summary>
    /// InsertAsync method.
    /// </summary>
public async Task InsertAsync(T model)
    {
        var res = await db.InsertAsync(model);
    }

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public async Task UpdateAsync(T model)
    {
        var res = await db.UpdateAsync(model);
    }

        /// <summary>
    /// UpsertAsync method.
    /// </summary>
public async Task UpsertAsync(T model)
    {
        var res = await db.UpsertAsync(model);
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task DeleteAsync(T model)
    {
        await DeleteAsync(model.Id);
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task DeleteAsync(long id)
    {
        await db.DeleteAsync(id);
    }

        /// <summary>
    /// FindAsync method.
    /// </summary>
public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        var results = await db.FindAsync((Expression<Func<AeroUserProfile, bool>>)(object)predicate);
        return results.Cast<T>();
    }
}