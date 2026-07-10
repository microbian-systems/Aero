using Aero.Core.Data;
using Aero.Marten;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aero.EfCore.Extensions;

/// <summary>
/// Represents a class for AeroDbExtensions.
/// </summary>
public static class AeroDbExtensions
{

        /// <summary>
    /// AddAeroDataLayer method.
    /// </summary>
public static IServiceCollection AddAeroDataLayer(
        this IServiceCollection services, 
        IConfiguration config, 
        IHostEnvironment env,
        Action<StoreOptions>? UpdateMartenOptions = null)
    {
        var migrationAssembly = typeof(AeroDbContext)
            //.GetTypeInfo()
            .Assembly
            .GetName().Name;

        // todo - store common connection string name in a constant somewhere and reference it here and in appsettings
        var connString = config.GetConnectionString(Schemas.Aero);


        services.AddDbContextPool<AeroDbContext>(o =>
                o.UseNpgsql(connString,
                    x => x.MigrationsHistoryTable(Schemas.MigrationTableName, Schemas.Aero)
                        .MigrationsAssembly(migrationAssembly)))
            //.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
            ;

        // todo - verify these DI service registrations are valid and test them
        // todo - do these DI registrations belong in the dbcontext registration
        // services.AddScoped(typeof(IGenericRepository<>), typeof(GenericEntityFrameworkRepository<>));
        // services.AddScoped(typeof(IGenericEntityFrameworkRepository<>), typeof(GenericEntityFrameworkRepository<>));
        // services.AddScoped(typeof(IGenericEntityFrameworkRepository<,>), typeof(GenericEntityFrameworkRepository<,>));
        services.AddScoped<IAiUsageLogRepository, AiUsageLogsRepository>();
        services.AddScoped<IApiAuthRepository, ApiAuthRepository>();

        // todo - rename this project from EfCore to Data and move Marten stuff in same project 
        services.AddScoped<IAeroDb, AeroDb>();
        services.AddScoped<IAeroUserRepository, AeroUserRepository>();

        return services;
    }
}
