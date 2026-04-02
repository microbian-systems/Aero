namespace Aero.Cms.Web.Core.Modules;

using Aero.Cms.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Defines the core contract for an Aero.Cms module functionality.
/// </summary>
public interface IAeroModule
{
    /// <summary>
    /// The name of the module
    /// </summary>
    string Name { get; }
    /// <summary>
    /// the version number of the product
    /// <remarks>important in troubleshooting and updating</remarks>
    /// </summary>
    string Version { get; }
    /// <summary>
    /// the author of the module
    /// </summary>
    string Author { get; }
    /// <summary>
    /// defines the order in which the module should be loaded
    /// <remarks>negative numbers are loaded before larger numbers.</remarks>
    /// <example>to run something last the order of 1MM would work</example>
    /// </summary>
    short Order { get ; }
    /// <summary>
    /// Signals if the module is enabled/disabled
    /// </summary>
    public bool Disabled { get; set; }
    /// <summary>
    /// Signals if the module is enabled/disabled in production environments
    /// </summary>
    public bool DisabledInProductions { get; set; }
    /// <summary>
    /// Description of the module
    /// </summary>
    public string? Description { get; }
    /// <summary>
    /// Urls that have information about the module
    /// </summary>
    Dictionary<string, Uri> Urls { get; } 

    /// <summary>
    /// for plugins that are not meant to be run in production
    /// </summary>
    bool DisabledInProduction { get; }

    /// <summary>
    /// any dependencies for the module
    /// </summary>
    IReadOnlyList<string> Dependencies { get; }
    /// <summary>
    /// the categories the module belongs to
    /// </summary>
    IReadOnlyList<string> Category { get; }
    /// <summary>
    /// the tags associated with the modules.
    /// <remarks>users can use this to discover modules</remarks>
    /// </summary>
    IReadOnlyList<string> Tags { get; }
    void Configure(IAeroModuleBuilder builder);
    /// <summary>
    /// configure the aspnet core services pipeline
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="config">The application configuration.</param>
    /// <param name="env">The current host environment.</param>
    void ConfigureServices(IServiceCollection services, IConfiguration? config = null, IHostEnvironment? env = null);
    /// <summary>
    /// configure the aspnet core middleware pipeline synchronously
    /// </summary>
    /// <param name="endpoints"><see cref="IEndpointRouteBuilder"/></param>
    void Run(IServiceProvider sp);
    /// <summary>
    /// configure the aspnet core middleware pipeline asynchronously
    /// </summary>
    /// <param name="endpoints"><see cref="IEndpointRouteBuilder"/></param>
    /// <returns>asynchronous task <see cref="Task"/></returns>
    Task RunAsync(IServiceProvider sp);
}


/// <summary>
/// UI modules for Aero
/// <remarks>add UI components to be used within the Aero CMS editor</remarks>
/// </summary>
public interface IUiModule : IAeroModule { }

/// <summary>
/// Modules that define API functionality for Aero
/// </summary>
public interface IApiModule : IAeroModule { }

/// <summary>
/// Used for Aero CMS background jobs
/// </summary>
public interface IBackgroundModule : IAeroModule { }

/// <summary>
/// Standard Aero CMS module
/// </summary>
public interface IThemeModule : IAeroModule { }

/// <summary>
/// Admin modules for Aero CMS
/// <remarks>adds functionality to the Aero CMS admin</remarks>
/// </summary>
public interface IAdminModule : IAeroModule { }

/// <summary>
/// Standard aspnet core filter modules
/// </summary>
public interface IFilterModule : IAeroModule { }

public interface IContentDefinitionModule : IAeroModule { }
