using Aero.Core.Entities;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Aero.Modular;

// todo - move aero module core system to its own project and package (aero.core.modules)
// so it can be shared with other types of applications (console, desktop, etc)

/// <summary>
/// A base class for Aero.Cms modules that provides default implementations.
/// </summary>
public abstract class AeroModuleBase : IAeroModule, IConfigureMarten, IDisposable
{
    /// <summary>
    /// Provides access to the logger instance used for logging diagnostic and operational information within the class.
    /// </summary>
    /// <remarks>Use this logger to record informational messages, warnings, errors, or other events relevant
    /// to the class's operation. The logger is initialized from the application's global logging
    /// configuration.</remarks>
    protected readonly ILogger log = Log.Logger;

    /// <inheritdoc/>
    public abstract string Name { get; }
    /// <inheritdoc/>
    public abstract string Version { get; }
    /// <inheritdoc/>
    public abstract string Author { get; }
    /// <inheritdoc/>
    public virtual short Order { get; } = 0;
    /// <inheritdoc/>
    public virtual Dictionary<string, Uri> Urls { get; } = [];
    /// <inheritdoc/>
    public abstract IReadOnlyList<string> Dependencies { get; }
    /// <inheritdoc/>
    public abstract IReadOnlyList<string> Category { get; }
    /// <inheritdoc/>
    public abstract IReadOnlyList<string> Tags { get; }
    /// <inheritdoc/>
    public virtual bool DisabledInProduction => false;
    /// <inheritdoc/>
    public virtual bool DisabledInProductions { get; set; }
    /// <inheritdoc/>
    public virtual string? Description => null;
    /// <inheritdoc/>
    public bool Disabled { get ; set ; }

    /// <inheritdoc/>
    public virtual void Configure(IAeroModuleBuilder builder)
    {
    }

    /// <inheritdoc/>
    public virtual void ConfigureServices(IServiceCollection services, IConfiguration? config = null, IHostEnvironment? env = null)
    {
    }

    /// <inheritdoc/>
    public virtual void Run(IServiceProvider sp) => RunAsync(sp).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public virtual Task RunAsync(IServiceProvider sp) => Task.CompletedTask;


    // todo - impl IAsyncDisposable pattern for modules
    public virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Release managed resources here if needed
        }
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Configure<T>(IServiceProvider services, StoreOptions opts, bool index = true)
        where T : Entity
    {
        opts.Schema.For<T>().Identity(x => x.Id);
        if (index == false) return;
        opts.Schema.For<T>().Index(x => x.CreatedBy);
        opts.Schema.For<T>().Index(x => x.ModifiedBy);
        opts.Schema.For<T>().Index(x => x.CreatedOn);
        opts.Schema.For<T>().Index(x => x.ModifiedOn);
    }

    // todo - add documentation for the marten configuration method and how it is used to configure document schemas, indexes, etc. and how it is called during application startup
    public virtual void Configure(IServiceProvider services, StoreOptions options)
    {
        
    }
}
