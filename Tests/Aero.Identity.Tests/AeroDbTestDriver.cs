using JasperFx;
using Marten;

namespace Aero.Identity.Tests;

/// <summary>
/// Represents a class for AeroDbTestDriver.
/// </summary>
public abstract class AeroDbTestDriver : IDisposable
{
        /// <summary>
    /// Gets or sets the store.
    /// </summary>
protected IDocumentStore store
    {
        get => field ?? GetDocumentStore();
        init;
    }

        /// <summary>
    /// Gets or sets the Is Disposed.
    /// </summary>
protected bool IsDisposed { get; private set; }

        /// <summary>
    /// Initializes a new instance of the <see cref="AeroDbTestDriver"/> class.
    /// </summary>
protected AeroDbTestDriver()
    {
        store = DocumentStore.For(opts =>
        {
            var connString = "Host=localhost;Port=5432;Database=aero-test;Username=postgres;Password=*strongPassword1;";
            opts.Connection(connString!);
            opts.AutoCreateSchemaObjects = AutoCreate.All; // Dev mode: create tables if missing
        });
        
        // Clean the database before each test run
        store.Advanced.Clean.DeleteAllDocumentsAsync().GetAwaiter().GetResult();
    }

        /// <summary>
    /// GetDocumentStore method.
    /// </summary>
protected internal IDocumentStore GetDocumentStore(string? database = null)
    {
        return this.store;
    }

        /// <summary>
    /// PreInitialize method.
    /// </summary>
protected virtual void PreInitialize(IDocumentStore store)
    {

    }

        /// <summary>
    /// PreConfigureDatabase method.
    /// </summary>
protected virtual void PreConfigureDatabase(IDocumentStore store)
    {

    }

        /// <summary>
    /// SetupDatabase method.
    /// </summary>
protected virtual void SetupDatabase(IDocumentStore store)
    {

    }

        /// <summary>
    /// Dispose method.
    /// </summary>
public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

        /// <summary>
    /// Dispose method.
    /// </summary>
protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed) return;
        if (disposing)
        {
            store.Dispose();
        }
        IsDisposed = true;
    }
}