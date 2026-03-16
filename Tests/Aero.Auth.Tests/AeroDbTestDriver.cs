using Marten;
using JasperFx;

namespace Aero.Auth.Tests;

public abstract class AeroDbTestDriver : IDisposable
{
    private static readonly IDocumentStore _sharedStore;
    protected readonly IDocumentStore store;

    static AeroDbTestDriver()
    {
        _sharedStore = DocumentStore.For(opts =>
        {
            var connString = "Host=localhost;Port=5432;Database=aero-test;Username=postgres;Password=*strongPassword1;";
            opts.Connection(connString);
            //opts.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;
        });
    }

    protected AeroDbTestDriver()
    {
        store = _sharedStore;
        
        // Clean the database before each test run
        store.Advanced.Clean.DeleteAllDocumentsAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        // Don't dispose the shared store here
        GC.SuppressFinalize(this);
    }
}
