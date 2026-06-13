using Aero.Core.Data;
using Aero.Core.Identity;
using Aero.Models.Entities;
using JasperFx;
using JasperFx.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aero.Marten.Extensions;

public static class MartenDbExtensions
{
    /// <summary>
    /// configures marten db and registers related services.
    /// adding event store support here as well since it is a core part of the library and 
    /// requires some specific configuration (e.g. StreamIdentity.AsString for string-based stream ids)
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="env"></param>
    /// <param name="connString"></param>
    /// <param name="UpdateMartenOptions"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureMartenDb(
        this IServiceCollection services,
        IConfiguration config,
        IHostEnvironment env,
        string? connString = null,
        Action<StoreOptions>? UpdateMartenOptions = null)
    {
        connString = !string.IsNullOrEmpty(connString) 
            ? connString
            : config.GetConnectionString(Schemas.Aero);
        // todo - move this to the application/client level - anything that needs IDocumentSession can get it via DI
        // and instantiation at this level is too low.  There are other indexes this library is not aware of that need to be added
        var _ = services.AddMarten(opts =>
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

            if (UpdateMartenOptions is not null)
                UpdateMartenOptions(opts);

            // enable automatic schema creation for development
            if (env.IsDevelopment())
                opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

            if(UpdateMartenOptions is not null)
                UpdateMartenOptions(opts);
        })
        .UseLightweightSessions()
        //.IntegrateWithWolverine()
        ;

        services.AddScoped<IDynamicMartenRepository, DynamicMartinRepository>();
        //services.AddScoped(typeof(IGenericMartenRepository<>), typeof(GenericMartenRepository<>));
        services.AddScoped<IAeroDb, AeroDb>();

        //if (host.IsDevelopment())
        //_.InitializeStore();
        _.InitializeWith(); // ported from another deprecated aero lib - verify what marten InitializeWith() does and if it is needed here

        return services;
    }
}
