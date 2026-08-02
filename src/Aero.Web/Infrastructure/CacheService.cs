using Microsoft.Extensions.Caching.Memory;

namespace Aero.Web.Infrastructure;

/// <summary>
/// Service used to resolve and invalidate API keys.
/// </summary>
public interface IApiKeysCacheService
{
    /// <summary>
    /// Gets a API Key Owner ID (owner of the API key) from its API Key.
    /// </summary>
    /// <param name="apiKey">The API Key received on the HTTP request.</param>
    /// <returns>The ID of the owner of the API Key if the key was found, null otherwise.</returns>
    ValueTask<string?> GetOwnerIdFromApiKey(string apiKey);

    /// <summary>
    /// Invalidates (removes from cache and/or permanent storage) an API key.
    /// </summary>
    /// <param name="apiKey">The API Key to invalidate</param>
    /// <returns>A task representing the operation.</returns>
    Task InvalidateApiKey(string apiKey);
}


/// <summary>
/// Represents a class for ApiKeyCacheService.
/// </summary>
public class ApiKeyCacheService(IMemoryCache memoryCache, IClientsService clientsService) : IApiKeysCacheService
{
    private static readonly TimeSpan cacheKeysTimeToLive = new(1, 0, 0);

        /// <summary>
    /// GetOwnerIdFromApiKey method.
    /// </summary>
public async ValueTask<string?> GetOwnerIdFromApiKey(string apiKey)
    {
        if (!memoryCache.TryGetValue<Dictionary<string, Guid>>("Authentication_ApiKeys", out var internalKeys))
        {
            internalKeys = await clientsService.GetActiveClients();

            memoryCache.Set("Authentication_ApiKeys", internalKeys, cacheKeysTimeToLive);
        }

        if (!internalKeys.TryGetValue(apiKey, out var clientId))
        {
            return null;
        }

        return clientId.ToString();
    }

        /// <summary>
    /// InvalidateApiKey method.
    /// </summary>
public async Task InvalidateApiKey(string apiKey)
    {
        if (memoryCache.TryGetValue<Dictionary<string, Guid>>("Authentication_ApiKeys", out var internalKeys))
        {
            if (internalKeys.ContainsKey(apiKey))
            {
                internalKeys.Remove(apiKey);
                memoryCache.Set("Authentication_ApiKeys", internalKeys);
            }
        }

        await clientsService.InvalidateApiKey(apiKey);
    }
}