using Aero.Auth.Jwt;
using Aero.Auth.Services;

namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for ApiServiceExtensions.
/// </summary>
public static class ApiServiceExtensions
{
        /// <summary>
    /// AddDefaultApiServices method.
    /// </summary>
public static WebApplicationBuilder AddDefaultApiServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDefaultApiServices(builder.Configuration);
        return builder;
    }

        /// <summary>
    /// AddDefaultApiServices method.
    /// </summary>
public static IServiceCollection AddDefaultApiServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<IJwtFactory, JwtFactory>();
        services.AddTransient<IClaimsPrincipalFactory, ClaimsPrincipalFactory>();
        services.AddScoped<IApiKeyFactory, DefaultApiKeyFactory>();

        return services;
    }
}