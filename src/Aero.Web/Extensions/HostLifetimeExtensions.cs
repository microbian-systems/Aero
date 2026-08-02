namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for HostLifetimeExtensions.
/// </summary>
public static class HostLifetimeExtensions
{
        /// <summary>
    /// AddLifetimeLogging method.
    /// </summary>
public static WebApplication AddLifetimeLogging(this WebApplication app, string appName = "")
    {
        var log = app.Services.GetRequiredService<ILogger<WebApplication>>();
        app.Lifetime.ApplicationStarted.Register(() => log.LogInformation("{appName} Background services have started", appName));
        app.Lifetime.ApplicationStopping.Register(() => log.LogInformation("{appName} Background services are stopping...", appName));
        app.Lifetime.ApplicationStopped.Register(() => log.LogInformation("{appName} Background services have stopped", appName));

        return app;
    }
}