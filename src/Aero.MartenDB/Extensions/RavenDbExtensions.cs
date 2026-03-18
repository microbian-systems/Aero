using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aero.MartenDB.Extensions;

public static class AeroDbExtensions
{
    public static IServiceCollection AddAeroPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Load AeroDB settings from configuration
        var AeroDbSettings = configuration.GetSection(AeroDbSettings.SectionName).Get<AeroDbSettings>() 
            ?? new AeroDbSettings();

        // 1. Register the DocumentStore as a SINGLETON
        // It is expensive to create and should exist once for the lifetime of the app.
        services.AddSingleton<IDocumentStore>(ctx =>
        {
            IDocumentStore store = null;

            if (AeroDbSettings.UseEmbedded)
            {
                // For embedded AeroDB, you need to add AeroDB.Embedded NuGet package
                // and uncomment the code below:

                //var embeddedOptions = new ServerOptions();

                if (!string.IsNullOrWhiteSpace(AeroDbSettings.EmbeddedPath))
                {
                    //embeddedOptions.DataDirectory = AeroDbSettings.EmbeddedPath;
                }

                //EmbeddedServer.Instance.StartServer(embeddedOptions);

                // store = EmbeddedServer.Instance.GetDocumentStore(
                //     new DatabaseOptions(AeroDbSettings.DatabaseName));
            }
            else
            {
                // Use server-based AeroDB
                var urls = AeroDbSettings.Urls?
                    .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(u => u.Trim())
                    .ToArray() ?? ["http://localhost:8080"];

                store = DocumentStore.For(opts =>
                {

                });
            }

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
        services.AddScoped<IAeroDbUnitOfWork, AeroUnitOfWork>();
        services.AddScoped<IAeroUserRepository>(ctx => 
            ctx.GetRequiredService<IAeroDbUnitOfWork>().Users);

        return services;
    }
}
