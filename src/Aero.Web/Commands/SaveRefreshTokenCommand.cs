using Aero.Core.Commands;
using Aero.Core.Entities;
using Aero.Marten;
using Aero.Models.Entities;

namespace Aero.Web.Commands;

// todo - move this to the marten cqs project
/// <summary>
/// Represents a class for SaveRefreshTokenCommand.
/// </summary>
public class SaveRefreshTokenCommand(
    IDynamicMartenRepository db,
    IAsyncCommand<DeleteRefreshTokenRequest, bool> command)
    : IAsyncCommand<SaveRefreshTokenRequest, bool>
{
        /// <summary>
    /// SaveRefreshToken method.
    /// </summary>
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

        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task<bool> ExecuteAsync(SaveRefreshTokenRequest parameter) => await SaveRefreshToken(parameter);
}

/// <summary>
/// Represents a class for SaveRefreshTokenRequest.
/// </summary>
public class SaveRefreshTokenRequest : Entity
{
        /// <summary>
    /// Gets or sets the User Id.
    /// </summary>
public long UserId { get; set; }
        /// <summary>
    /// Gets or sets the Username.
    /// </summary>
public string Username { get; set; }
        /// <summary>
    /// Gets or sets the Password.
    /// </summary>
public string Password { get; set; }
        /// <summary>
    /// Gets or sets the Token.
    /// </summary>
public string Token { get; set; }
}