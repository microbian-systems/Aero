
// todo - consider moving LoggerConfig into its own csproj

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for ConfigurationExtensions.
/// </summary>
public static class ConfigurationExtensions
{
        /// <summary>
    /// AddConfiguration method.
    /// </summary>
public static ConfigurationManager AddConfiguration<T>(this ConfigurationManager config, IHostEnvironment env)
        where T : class
    {
        config.AddJsonFile("appsettings.json", true);
        config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);
        config.AddUserSecrets<T>();
        config.AddEnvironmentVariables();

        return config;
    }
}

/// <summary>
/// Represents a class for LoggingExtensions.
/// </summary>
public static class LoggingExtensions
{
    const string fileLogPath = "logs/aero-.log";
    static ReloadableLogger? log = null;
    // may not need this if loading everything before the module initialization
    static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
    /// ConfigureLogging method.
    /// </summary>
public static async Task<ReloadableLogger> ConfigureLogging(this IServiceCollection services, IConfiguration config, string appName = "Aero")
    {
        if (appName.IsNullOrEmpty())
            appName = config["AppName"] ?? "aero-app-01";

        if (log is not null)
            return log;

        await semaphore.WaitAsync();

        try
        {
            if (log is not null)
                return log;

            var logConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", appName)
                .WriteTo.Console()
                .WriteTo.File(fileLogPath, rollingInterval: RollingInterval.Day)
                //.CreateBootstrapLogger()
                ;

            log = logConfig.CreateBootstrapLogger();

            services.AddSerilog(log);
        }
        finally
        {
            semaphore.Release();
        }

        return log;
    }
}