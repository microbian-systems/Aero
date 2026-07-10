using System.Diagnostics;

namespace Aero.Web.Middleware;

/// <summary>
/// Represents a class for PerfLoggingMiddleware.
/// </summary>
public class PerfLoggingMiddleware(RequestDelegate next, ILogger<PerfLoggingMiddleware> log)
{
        /// <summary>
    /// InvokeAsync method.
    /// </summary>
public async Task InvokeAsync(HttpContext context)
    {
        var sw = new Stopwatch();
        sw.Start();
        // Call the next delegate/middleware in the pipeline
        await next(context);
        sw.Stop();
        log.LogInformation(
            $"PerfMon - {context.Request.Protocol} request for {context.Request.Path} took {sw.ElapsedMilliseconds} ms");
    }
}

/// <summary>
/// Represents a class for PerfLoggingMiddlewareRegistration.
/// </summary>
public static class PerfLoggingMiddlewareRegistration
{
        /// <summary>
    /// UsePerfLogging method.
    /// </summary>
public static IApplicationBuilder UsePerfLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<PerfLoggingMiddleware>();
}