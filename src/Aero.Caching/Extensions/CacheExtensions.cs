using Microsoft.Extensions.DependencyInjection;

namespace Aero.Caching.Extensions;

public static class CacheExtensions
{
    public static IServiceCollection AddAeroCaching(this IServiceCollection services, bool useRedis = true)
    {
        if(useRedis)
            services.AddStackExchangeRedisCache(opts =>
                {
                    opts.Configuration = "localhost:6379";
                });
        
        services.AddScoped<ICacheService, FusionCacheClient>();
        services.AddScoped<IFusionCacheClient, FusionCacheClient>();
        
        return services;
    }
}