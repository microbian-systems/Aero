using Aero.Common.Commands;
using Aero.Core.Data;
using Aero.Core.Extensions;
using Aero.Models.Entities;


namespace Aero.Marten;

public class UpdateUserProfileCommand(
    IGenericRepository<AeroUserProfile, long> db,
    ILogger<UpdateUserProfileCommand> log)
    : IAsyncCommand<AeroUserProfile, AeroUserProfile>
{
    public async Task<AeroUserProfile> ExecuteAsync(AeroUserProfile profile)
    {
        log.LogInformation($"updating user profile: {profile.ToJson()}");
        var results = await db.UpsertAsync(profile);
        return results;
    }
}