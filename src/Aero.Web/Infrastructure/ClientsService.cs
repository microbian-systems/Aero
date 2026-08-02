namespace Aero.Web.Infrastructure;

/// <summary>
/// Defines an interface for IClientsService.
/// </summary>
public interface IClientsService
{
        /// <summary>
    /// GetActiveClients method.
    /// </summary>
Task<Dictionary<string, Guid>> GetActiveClients();
        /// <summary>
    /// InvalidateApiKey method.
    /// </summary>
Task InvalidateApiKey(string apiKey);
}

/// <summary>
/// Represents a class for ClientsService.
/// </summary>
public class ClientsService : IClientsService
{
        /// <summary>
    /// GetActiveClients method.
    /// </summary>
public async Task<Dictionary<string, Guid>> GetActiveClients()
    {
        throw new NotImplementedException();
    }

        /// <summary>
    /// InvalidateApiKey method.
    /// </summary>
public async Task InvalidateApiKey(string apiKey)
    {
        throw new NotImplementedException();
    }
}