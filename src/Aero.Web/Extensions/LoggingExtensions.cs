using Foundatio.Utility;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for LoggingExtensions.
/// </summary>
public static class LoggingExtensions
{
        /// <summary>
    /// AddDefaultLogging method.
    /// </summary>
public static WebApplicationBuilder AddDefaultLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, logConfig) =>
        {
            logConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.WithEnvironmentName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.FromLogContext();
        });

        AddDefaultLogging(builder.Services, builder.Configuration);

        return builder;
    }

        /// <summary>
    /// AddDefaultLogging method.
    /// </summary>
public static IServiceCollection AddDefaultLogging(this IServiceCollection services, IConfiguration config)
    {
        services.AddSerilogLogging(config);

        return services;
    }

        /// <summary>
    /// GetReloadableLogger method.
    /// </summary>
public static ILogger GetReloadableLogger(this IServiceCollection builder, IConfiguration config)
        => GetReloadableLogger(config);

        /// <summary>
    /// GetReloadableLogger method.
    /// </summary>
public static ILogger GetReloadableLogger(this WebApplicationBuilder builder)
        => GetReloadableLogger(builder.Configuration);

        /// <summary>
    /// GetReloadableLogger method.
    /// </summary>
public static ILogger GetReloadableLogger(IConfiguration config)
    {
        var log = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext()
            .CreateBootstrapLogger()
            .GetLogger();

        return log;
    }

        /// <summary>
    /// AddSerilogLogging method.
    /// </summary>
public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration config)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.WithEnvironmentName()
            .Enrich.WithEnvironmentUserName()
            .Enrich.FromLogContext();
        var logger = loggerConfig.CreateLogger();
        Log.Logger = logger;
        services.AddLogging(c =>
        {
            c.ClearProviders();
            c.AddConsole();
            //if (!config.Environment.IsProduction())
            c.AddDebug();

            c.AddSerilog(logger, dispose: true);
            var log = logger.GetLogger();
            log.LogInformation("Logging configured");
        });

        return services;
    }

        /// <summary>
    /// AddSerilogLogging method.
    /// </summary>
public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddSerilogLogging(builder.Configuration);

        return builder;
    }

    // public static IApplicationBuilder UseDefaultLogging(this IApplicationBuilder app)
    // {
    //     app.UseSerilogRequestLogging(options =>
    //     {
    //         // Customize the message template
    //         options.MessageTemplate = "Handled {RequestPath}";
    //
    //         // Emit debug-level events instead of the defaults
    //         var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
    //         if(env.IsDevelopment())
    //             options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;
    //
    //         // Attach additional properties to the request completion event
    //         options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    //         {
    //             diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
    //             diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
    //         };
    //     });
    //
    //     return app;
    // }
}