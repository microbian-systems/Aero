using Aero.Core.Data;
using Aero.Core.Identity;
using Aero.Marten;
using JasperFx;
using JasperFx.Events;
using Marten;
using Marten.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aero.EfCore.Extensions;

public static class AeroDbExtensions
{

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

        var connString = config.GetConnectionString("aero");


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


        // var store = DocumentStore.For(c =>
        // {
        //     c.DatabaseSchemaName = Schemas.Aero;
        //     c.Connection(connString!);
        // });

        // todo - move this to the application/client level - anything that needs IDocumentSession can get it via DI
        // and instantiation at this level is too low.  There are other indexes this library is not aware of that need to be added
        services.AddMarten(opts =>
                {
                    opts.Connection(connString!);
                    opts.DatabaseSchemaName = Schemas.Aero;
                    opts.Events.StreamIdentity = StreamIdentity.AsString;

                    opts.UseSystemTextJsonForSerialization(configure: o =>
                    {
                        // Required for [JsonDerivedType] / [JsonPolymorphic] with PostgreSQL jsonb.
                        // jsonb doesn't guarantee property order, so the type discriminator (e.g. $blockType)
                        // can appear at any position in the JSON object. Without this, STJ throws:
                        // "must specify a type discriminator" on deserialization.
                        o.AllowOutOfOrderMetadataProperties = true;
                    });
                    opts.Schema.For<AeroRole>().Identity(x => x.Id);
                    opts.Schema.For<AeroUser>().Identity(x => x.Id);

                    if(UpdateMartenOptions is not null)
                        UpdateMartenOptions(opts);

                    // enable automatic schema creation for development
                    if (env.IsDevelopment())
                        opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
                })
            .UseLightweightSessions();

        // todo - rename this project from EfCore to Data and move Marten stuff in same project 
        services.AddScoped<IAeroDb, AeroDb>();
        services.AddScoped<IAeroUserRepository, AeroUserRepository>();

        return services;
    }
}
