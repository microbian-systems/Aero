// Fixtures/TestWebAppFactory.cs

using Marten;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aero.Auth.Tests;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    protected readonly RavenDriver driver = new();
    protected IDocumentStore store;
    protected class RavenDriver : RavenTestDriver
    {
        internal IDocumentStore GetDocumentStore()
        {
            var store = GetDocumentStore();
            return store;
        }
    }


    protected override IHost CreateHost(IHostBuilder builder)
    {
        // 1. Initialize a clean, isolated document store for this test run
        store = driver.GetDocumentStore();

        // Optionally configure additional DI for test overrides
        builder.ConfigureServices((ctx, services) =>
        {
            var env = ctx.HostingEnvironment;
            var config = ctx.Configuration;
            services.AddSingleton<IDocumentStore>(store);
            services.AddScoped<IDocumentSession>(sp =>
            {
                var session = store.LightweightSession();
                return session;
            });
        });

            

        return base.CreateHost(builder);
    }
}