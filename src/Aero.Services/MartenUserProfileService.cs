using Aero.Core.Extensions;
using Aero.Marten;
using System.Linq.Expressions;


namespace Aero.Services;

/// <summary>
/// Represents a class for MartenUserProfileService.
/// </summary>
public sealed class MartenUserProfileService<T>(
    IUserRepository userRepository,
    IGenericMartenRepository<T, long> db,
    ILogger<MartenUserProfileService<T>> log)
    : IUserProfileService<T>
    where T : AeroUserProfile, new()
{
        /// <summary>
    /// GetById method.
    /// </summary>
public async Task<T> GetById(long id)
    {
        log.LogInformation($"getting user profile with id: {id}");
        return await db.FindByIdAsync(id);
    }

        /// <summary>
    /// GetByEmail method.
    /// </summary>
public async Task<T> GetByEmail(string email)
    {
        var user = (await userRepository.FindAsync(x => x.Email == email))
            .FirstOrDefault();

        if (user is null)
            return null;

        // todo - temporary fix for compilation: get the actual user profile from the db
        var profile = new AeroUserProfile(); // user.Profile;
        return (T)profile;
    }

        /// <summary>
    /// InsertAsync method.
    /// </summary>
public async Task InsertAsync(T model)
    {
        log.LogInformation($"adding user: {model.ToJson()}");
        await db.InsertAsync(model);
        await db.SaveChangesAsync();
    }

        /// <summary>
    /// UpdateAsync method.
    /// </summary>
public async Task UpdateAsync(T model)
    {
        log.LogInformation($"updating user: {model.ToJson()}");
        await db.UpdateAsync(model);
        await db.SaveChangesAsync();
    }

        /// <summary>
    /// UpsertAsync method.
    /// </summary>
public async Task UpsertAsync(T model)
    {
        log.LogInformation($"upserting user: {model.ToJson()}");
        await db.UpsertAsync(model);
        await db.SaveChangesAsync();
    }

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task DeleteAsync(T model) => await DeleteAsync(model.Id);

        /// <summary>
    /// DeleteAsync method.
    /// </summary>
public async Task DeleteAsync(long id)
    {
        log.LogWarning($"deleting user with id {id}");
        await db.DeleteAsync(id);
        await db.SaveChangesAsync();
    }

        /// <summary>
    /// FindAsync method.
    /// </summary>
public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await db.FindAsync(predicate);
    }
}