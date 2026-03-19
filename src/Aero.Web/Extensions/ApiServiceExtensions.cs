using Aero.Common.Web.Infrastructure;
using Aero.Common.Web.Jwt;

namespace Aero.Common.Web.Extensions;

public static class ApiServiceExtensions
{
    public static WebApplicationBuilder AddDefaultApiServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDefaultApiServices(builder.Configuration);
        return builder;
    }

    public static IServiceCollection AddDefaultApiServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<IJwtFactory, JwtFactory>();
        services.AddTransient<IClaimsPrincipalFactory, ClaimsPrincipalFactory>();
        services.AddScoped<IApiKeyFactory, DefaultApiKeyFactory>();

        return services;
    }
}