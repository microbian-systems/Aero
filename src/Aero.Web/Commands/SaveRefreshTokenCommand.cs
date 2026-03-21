using Aero.Models.Entities;
using Aero.Common.Commands;
using Aero.Core.Entities;
using Aero.Marten;

namespace Aero.Common.Web.Commands;

// todo - move this to the marten cqs project
public class SaveRefreshTokenCommand : IAsyncCommand<SaveRefreshTokenRequest, bool>
{
    private readonly IDynamicMartenRepository db;
    private readonly IAsyncCommand<DeleteRefreshTokenRequest, bool> command;

    public SaveRefreshTokenCommand(IDynamicMartenRepository db, IAsyncCommand<DeleteRefreshTokenRequest, bool> command)
    {
        this.db = db;
        this.command = command;
    }
        
    public async Task<bool> SaveRefreshToken(SaveRefreshTokenRequest request)
    {
        var success = await command.ExecuteAsync(new DeleteRefreshTokenRequest()
        {
            Username = request.Username,
            Password = request.Password
        });
        var entity = new RefreshToken
        {
            TokenHash = request.Token, 
            UserId = request.UserId,
            CreatedOn = DateTimeOffset.UtcNow,
            ModifiedOn =  DateTimeOffset.UtcNow
        };
            
        await db.SaveAsync(entity);
        return true;
    }

    public async Task<bool> ExecuteAsync(SaveRefreshTokenRequest parameter) => await SaveRefreshToken(parameter);
}

public class SaveRefreshTokenRequest : Entity
{
    public long UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Token { get; set; }
}