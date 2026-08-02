using Aero.Caching.Decorators;

namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for CachingExtensions.
/// </summary>
public static class CachingExtensions
{
        /// <summary>
    /// AddAeroCaching method.
    /// </summary>
public static IServiceCollection AddAeroCaching(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped(typeof(ICachingRepositoryDecorator<,>), typeof(CachingRepository<,>));
        services.AddScoped(typeof(ICachingRepositoryDecorator<>), typeof(CachingRepository<>));
        return services;
    }
}