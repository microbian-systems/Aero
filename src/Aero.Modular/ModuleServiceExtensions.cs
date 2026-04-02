namespace Aero.Cms.Web.Core.Modules;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering module system services.
/// </summary>
public static class ModuleServiceExtensions
{
    /// <summary>
    /// Adds module system services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration for discovery options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddModuleSystem(
        this IServiceCollection services,
        Action<ModuleDiscoveryOptions>? configureOptions = null)
    {
        // Register options
        services.AddOptions<ModuleDiscoveryOptions>();
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        // Register core module services
        services.AddScoped<IModuleDiscoveryService, ModuleDiscoveryService>();
        services.AddScoped<IModuleGraphService, ModuleGraphService>();

        return services;
    }

    /// <summary>
    /// Adds module system services with configuration from the app settings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddModuleSystem(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ModuleDiscoveryOptions>(configuration.GetSection("ModuleDiscovery"));

        // Register core module services
        services.AddScoped<IModuleDiscoveryService, ModuleDiscoveryService>();
        services.AddScoped<IModuleGraphService, ModuleGraphService>();

        return services;
    }
}
