using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aero.Core.Logging;

public static class LoggingExtensions
{
    public static IHostApplicationBuilder AddAeroLogging(this IHostApplicationBuilder builder)
    {
        // 1. Setup the Bootstrap logger (same as before)
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        // 2. Clear default providers and add Console if desired
        // In the new builder, you access Logging directly via builder.Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        if(builder.Environment.IsDevelopment())
            builder.Logging.AddDebug();

        // 3. Use Serilog
        // This requires the 'Serilog.Extensions.Hosting' NuGet package
        builder.Services.AddSerilog((services, configuration) => configuration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        return builder;
    }
}
