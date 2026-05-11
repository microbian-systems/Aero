using Aero.Marten;
using JasperFx;
using JasperFx.Events;
using Marten;
using Marten.Events;

namespace Aero.Web.Extensions;

public static class MartenExtensions
{
    public static IServiceCollection ConfigureMarten(this IServiceCollection services, IConfiguration config, IWebHostEnvironment host)
    {
        var connString = config.GetConnectionString("aero");

        var marten = services.AddMarten(opts =>
        {
            opts.Connection(connString);
            opts.Events.StreamIdentity = StreamIdentity.AsString;
            if (host.IsDevelopment())
            {
                opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
            }
        })
        .UseLightweightSessions();

        services.AddScoped<IDynamicMartenRepository, DynamicMartinRepository>();
        services.AddScoped(typeof(IGenericMartenRepository<>), typeof(GenericMartenRepository<>));

        //if (host.IsDevelopment())
        //marten.InitializeStore();
        marten.InitializeWith();

        return services;
    }
}
