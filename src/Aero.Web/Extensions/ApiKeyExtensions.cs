using Aero.Auth.Services;
using Aero.Common.Web.Infrastructure;
using Aero.Common.Web.Services;

namespace Aero.Common.Web.Extensions;

/// <summary>
/// Represents a class for ApiKeyExtensions.
/// </summary>
public static class ApiKeyExtensions
{
        /// <summary>
    /// AddApiKeyGenerator method.
    /// </summary>
public static WebApplicationBuilder AddApiKeyGenerator(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        builder.Services.AddApiKeyGenerator(config);
        return builder;
    }
    
        /// <summary>
    /// AddApiKeyGenerator method.
    /// </summary>
public static IServiceCollection AddApiKeyGenerator(
        this IServiceCollection services,
        IConfiguration config)
    {
        const string apiKeySection = "apiKeyOptions";
        var apiKeyOptions = new ApiKeyOptions();
        config.GetSection(apiKeySection).Bind(apiKeyOptions);
        services.Configure<ApiKeyOptions>(config.GetSection(apiKeySection));
        
        services.AddTransient<IApiKeyService, ApiKeyService>();
        services.AddTransient<IApiKeyFactory, DefaultApiKeyFactory>();
        
        return services;
    }
}