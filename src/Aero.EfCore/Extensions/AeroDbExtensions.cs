using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aero.EfCore.Extensions;

/// <summary>
/// Provides extension methods for configuring the Aero data layer.
/// EF Core Npgsql has been fully replaced by AeroDB.Sable.
/// </summary>
public static class AeroDbExtensions
{
    /// <summary>
    /// Registers Aero data layer services. EF Core and Npgsql registrations removed;
    /// all persistence now uses AeroDB.Sable via <c>IDocumentSession</c>.
    /// </summary>
    public static IServiceCollection AddAeroDataLayer(
        this IServiceCollection services,
        IConfiguration config,
        IHostEnvironment env)
    {
        // All EF Core Npgsql registrations removed.
        // AeroDbContext, ApiAuthRepository, AiUsageLogsRepository, AuthInitializationExtensions deleted.
        // Persistence handled by AeroDB.Sable (IDocumentSession / IDocumentStore).
        return services;
    }
}
