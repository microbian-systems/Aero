namespace Aero.Web.SignalR;

/// <summary>
/// Represents a class for SignalRConfigExtensions.
/// </summary>
public static class SignalRConfigExtensions
{
        /// <summary>
    /// ConfigureSignalR method.
    /// </summary>
public static IApplicationBuilder ConfigureSignalR(this IApplicationBuilder app)
    {
        var log = app.ApplicationServices.GetRequiredService<ILogger<ChatHub>>();
        log.LogInformation($"configuring signalr hubs....");

        app.UseEndpoints(ep =>
        {
            ep.MapHub<ChatHub>("/chathub");
        });

        log.LogInformation($"finished configuring signalr hubs");
        
        return app;
    }
}