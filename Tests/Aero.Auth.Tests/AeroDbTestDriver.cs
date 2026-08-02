using Marten;
using JasperFx;

namespace Aero.Auth.Tests;

/// <summary>
/// Represents a class for AeroDbTestDriver.
/// </summary>
public abstract class AeroDbTestDriver : IDisposable
{
    private static readonly IDocumentStore _sharedStore;
        /// <summary>
    /// store.
    /// </summary>
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

        /// <summary>
    /// Initializes a new instance of the <see cref="AeroDbTestDriver"/> class.
    /// </summary>
protected AeroDbTestDriver()
    {
        store = _sharedStore;
        
        // Clean the database before each test run
        store.Advanced.Clean.DeleteAllDocumentsAsync().GetAwaiter().GetResult();
    }

        /// <summary>
    /// Dispose method.
    /// </summary>
public void Dispose()
    {
        // Don't dispose the shared store here
        GC.SuppressFinalize(this);
    }
}
