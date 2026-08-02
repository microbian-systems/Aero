using System.Globalization;

namespace Aero.Web.Middleware;

/// <summary>
/// Represents a class for RequestCultureMiddleware.
/// </summary>
public class RequestCultureMiddleware(RequestDelegate next)
{
        /// <summary>
    /// InvokeAsync method.
    /// </summary>
public async Task InvokeAsync(HttpContext context)
    {
        var cultureQuery = context.Request.Query["culture"];
        if (!string.IsNullOrWhiteSpace(cultureQuery))
        {
            var culture = new CultureInfo(cultureQuery);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        // Call the next delegate/middleware in the pipeline.
        await next(context);
    }
}

/// <summary>
/// Represents a class for RequestCultureMiddlewareExtensions.
/// </summary>
public static class RequestCultureMiddlewareExtensions
{
        /// <summary>
    /// UseRequestCultureMiddleware method.
    /// </summary>
public static IApplicationBuilder UseRequestCultureMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestCultureMiddleware>();
    }
}