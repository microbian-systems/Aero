using Aero.Core.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Aero.EfCore.Extensions;

public static class DbContextExtensions
{

    public static IServiceCollection AddApiAuthDbContext(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        var migrationAssembly = typeof(ApiAuthContext)
            .GetTypeInfo()
            .Assembly
            .GetName().Name;

        services.AddDbContextPool<ApiAuthContext>(o =>
                o.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__apiAuthMigrations", "apiauth")
                        .MigrationsAssembly(migrationAssembly)))
            //.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
            ;

        // todo - verify these DI service registrations are valid and test them
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericEntityFrameworkRepository<>));
        services.AddScoped(typeof(IGenericEntityFrameworkRepository<>), typeof(GenericEntityFrameworkRepository<>));
        services.AddScoped(typeof(IGenericEntityFrameworkRepository<,>), typeof(GenericEntityFrameworkRepository<,>));
        services.AddScoped<IAiUsageLogRepository, AiUsageLogsRepository>();
        services.AddScoped<IApiAuthRepository, ApiAuthRepository>();

        return services;
    }
}