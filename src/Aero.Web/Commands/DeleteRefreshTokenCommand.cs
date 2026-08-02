using Aero.Core.Commands;
using Aero.Marten;
using Aero.Models.Entities;

namespace Aero.Web.Commands;

/// <summary>
/// Represents a class for DeleteRefreshTokenRequest.
/// </summary>
public class DeleteRefreshTokenRequest
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
    /// Gets or sets the Refresh Token.
    /// </summary>
public string RefreshToken { get; set; }
}
    
/// <summary>
/// Represents a class for DeleteRefreshTokenCommand.
/// </summary>
public class DeleteRefreshTokenCommand(IDynamicMartenRepository db) : IAsyncCommand<DeleteRefreshTokenRequest, bool>
{
        /// <summary>
    /// DeleteRefreshToken method.
    /// </summary>
public async Task<bool> DeleteRefreshToken(long id, string refreshToken)
    {
        var record = await db.FindSingle<RefreshToken>(x => x.UserId == id);
        if (record != null)
            await db.DeleteAsync(record);
        return true;
    }

        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task<bool> ExecuteAsync(DeleteRefreshTokenRequest command) =>
        await DeleteRefreshToken(command.UserId, command.Password);
}