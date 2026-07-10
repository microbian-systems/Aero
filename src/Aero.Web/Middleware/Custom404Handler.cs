using System.Text.RegularExpressions;

namespace Aero.Web.Middleware;

/// <summary>
/// Represents a class for Custom404Handler.
/// </summary>
public class Custom404Handler
{
    private readonly RequestDelegate next;
    private readonly Regex apiRegex;

        /// <summary>
    /// Initializes a new instance of the <see cref="Custom404Handler"/> class.
    /// </summary>
public Custom404Handler(RequestDelegate next)
    {
        this.next = next;

        // Precompile the regex pattern for matching API routes
        var apiPattern = @"^/api/v\d{1,}/";
        apiRegex = new Regex(apiPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

        /// <summary>
    /// InvokeAsync method.
    /// </summary>
public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        // Check if the response status code is 404 (Not Found)
        if (context.Response.StatusCode == StatusCodes.Status404NotFound)
        {
            // Check if the request path matches an API pattern
            if (!IsApiRequest(context.Request.Path))
            {
                // Redirect to a custom error page or handle the not found scenario here
                context.Response.Redirect("/404", false);
            }
        }
    }

    private bool IsApiRequest(string requestPath)
    {
        // Check if the request path matches the precompiled API regex pattern
        return apiRegex.IsMatch(requestPath);
    }
}

/// <summary>
/// Represents a class for Custom404Extensions.
/// </summary>
public static class Custom404Extensions
{
        /// <summary>
    /// UseCustom404Handler method.
    /// </summary>
public static IApplicationBuilder UseCustom404Handler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<Custom404Handler>();
    }
}