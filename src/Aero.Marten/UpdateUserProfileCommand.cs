using Aero.Core.Commands;
using Aero.Core.Data;
using Aero.Core.Extensions;
using Aero.Models.Entities;


namespace Aero.Marten;

/// <summary>
/// Represents a class for UpdateUserProfileCommand.
/// </summary>
public class UpdateUserProfileCommand(
    IGenericRepository<AeroUserProfile, long> db,
    ILogger<UpdateUserProfileCommand> log)
    : IAsyncCommand<AeroUserProfile, AeroUserProfile>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task<AeroUserProfile> ExecuteAsync(AeroUserProfile profile)
    {
        log.LogInformation($"updating user profile: {profile.ToJson()}");
        var results = await db.UpsertAsync(profile);
        return results;
    }
}