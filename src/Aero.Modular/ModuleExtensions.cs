namespace Aero.Cms.Web.Core.Modules;

using Aero.Cms.Core.Modules;
using Aero.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Marten;
using Aero.Cms.Abstractions.Blocks;

// todo - abstract/extract Aero modules into its own lib so it can be used in any type of app (host, console, web, etc)


/// <summary>
/// Exception thrown when the module system fails during startup.
/// </summary>
public class ModuleSystemStartupException : Exception
{
    public ModuleSystemStartupException(string message) : base(message) { }
    public ModuleSystemStartupException(string message, Exception inner) : base(message, inner) { }
}
