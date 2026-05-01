using Aero.Core.Commands;
using Aero.Marten;
using Aero.Models.Entities;

namespace Aero.Web.Commands;

public class DeleteRefreshTokenRequest
{
    public long UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string RefreshToken { get; set; }
}
    
public class DeleteRefreshTokenCommand(IDynamicMartenRepository db) : IAsyncCommand<DeleteRefreshTokenRequest, bool>
{
    public async Task<bool> DeleteRefreshToken(long id, string refreshToken)
    {
        var record = await db.FindSingle<RefreshToken>(x => x.UserId == id);
        if (record != null)
            await db.DeleteAsync(record);
        return true;
    }

    public async Task<bool> ExecuteAsync(DeleteRefreshTokenRequest command) =>
        await DeleteRefreshToken(command.UserId, command.Password);
}