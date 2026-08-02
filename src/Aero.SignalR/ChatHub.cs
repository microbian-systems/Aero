using Microsoft.AspNetCore.SignalR;

namespace Aero.SignalR;

/// <summary>
/// Represents a class for ChatHub.
/// </summary>
public class ChatHub : Hub
{
        /// <summary>
    /// SendMessage method.
    /// </summary>
public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}