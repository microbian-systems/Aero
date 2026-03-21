using Aero.Core.Data;
using Aero.Core.Identity;
using Aero.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Aero.MartenDB;
using Marten;

namespace Aero.EfCore.Extensions;

public static class DbContextExtensions
{

    public static IServiceCollection AddAeroDataLayer(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        var migrationAssembly = typeof(AeroApiContext)
            //.GetTypeInfo()
            .Assembly
            .GetName().Name;

        var connString = config.GetConnectionString("aero");
        services.AddDbContextPool<AeroApiContext>(o =>
                o.UseNpgsql(connString,
                    x => x.MigrationsHistoryTable("__aeroApiMigrations", Schemas.Api)
                        .MigrationsAssembly(migrationAssembly)))
            //.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
            ;

        services.AddDbContextPool<AeroDbContext>(o =>
                o.UseNpgsql(connString,
                    x => x.MigrationsHistoryTable("__aeroMigrations", Schemas.Api)
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


        var store = DocumentStore.For(c =>
        {
            c.DatabaseSchemaName = Schemas.Aero;
            c.Connection(connString!);
        });

        services.AddMarten(opts =>
        {
            opts.Connection(connString!);
            opts.Schema.For<AeroRole>().Identity(x => x.Id);
            opts.Schema.For<AeroUser>().Identity(x => x.Id);
            // Optional: enable automatic schema creation for development
            //opts.AutoCreateSchemaObjects = SchemaMode.Development;
        });

        // todo - rename this project from EfCore to Data and move Marten stuff in same project 
        services.AddScoped<IAeroDb, AeroDb>();
        services.AddScoped<IAeroUserRepository>(ctx =>
            ctx.GetRequiredService<IAeroDb>().Users);

        return services;
    }
}
