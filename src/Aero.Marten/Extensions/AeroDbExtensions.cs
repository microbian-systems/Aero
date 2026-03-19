using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aero.MartenDB.Extensions;

public static class AeroDbExtensions
{
    public static IServiceCollection AddAeroPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Load AeroDB settings from configuration
        var aeroDbSettings = configuration.GetSection(AeroDbSettings.SectionName).Get<AeroDbSettings>() 
            ?? new AeroDbSettings();

        var connString = configuration.GetConnectionString("pgsql");

        if (aeroDbSettings.UseEmbedded)
        {
            return services;
        }

        // 1. Register the DocumentStore as a SINGLETON
        // It is expensive to create and should exist once for the lifetime of the app.
        services.AddSingleton<IDocumentStore>(ctx =>
        {
            IDocumentStore store = null;


                // Use server-based AeroDB
                var urls = aeroDbSettings.Hosts?
                    .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(u => u.Trim())
                    .ToArray() ?? ["http://localhost:8080"];

                store = DocumentStore.For(opts =>
                {

                });
            

            //store.Initialize();
            return store;
        });

        // 2. Register the Session as SCOPED
        // This creates a new session for every HTTP request.
        services.AddScoped<IDocumentSession>(sp =>
        {
            var store = sp.GetRequiredService<IDocumentStore>();
            return store.LightweightSession();
        });
        services.AddScoped<IDocumentSession>(ctx =>
        {
            var store = ctx.GetRequiredService<IDocumentStore>();
            return store.LightweightSession();
        });

        // 3. Register your Unit of Work as SCOPED
        // It depends on the Scoped session above.
        services.AddScoped<IAeroDb, AeroDb>();
        services.AddScoped<IAeroUserRepository>(ctx => 
            ctx.GetRequiredService<IAeroDb>().Users);

        return services;
    }
}
