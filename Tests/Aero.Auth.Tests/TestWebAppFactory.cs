using Marten;
using JasperFx;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aero.Auth.Tests;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    private static readonly IDocumentStore _sharedStore;

    static TestWebAppFactory()
    {
        var connString = "Host=localhost;Port=5432;Database=aero-test;Username=postgres;Password=*strongPassword1;";
        
        _sharedStore = DocumentStore.For(opts =>
        {
            opts.Connection(connString);
            //opts.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Clean the database
        _sharedStore.Advanced.Clean.DeleteAllDocumentsAsync().GetAwaiter().GetResult();

        builder.ConfigureServices((ctx, services) =>
        {
            services.AddSingleton<IDocumentStore>(_sharedStore);
            services.AddScoped<IDocumentSession>(sp => _sharedStore.LightweightSession());
        });

        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        // Don't dispose shared store
        base.Dispose(disposing);
    }
}
