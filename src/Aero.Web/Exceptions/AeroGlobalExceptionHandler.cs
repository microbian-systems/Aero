using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace Aero.Web.Exceptions;

/// <summary>
/// Represents a class for AeroGlobalExceptionHandler.
/// </summary>
public sealed class AeroGlobalExceptionHandler(
    ILogger<AeroGlobalExceptionHandler> logger,
    IHostEnvironment environment,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
        /// <summary>
    /// TryHandleAsync method.
    /// </summary>
public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var path = httpContext.Request.Path;

        logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}, Method: {Method}, Path: {Path}",
            traceId,
            httpContext.Request.Method,
            path);

        if (IsApiRequest(path))
        {
            await HandleApiExceptionAsync(
                httpContext,
                traceId,
                exception,
                cancellationToken);

            return true;
        }

        HandleNonApiException(httpContext, traceId);
        return true;
    }

    private async Task HandleApiExceptionAsync(
        HttpContext httpContext,
        string traceId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://httpstatuses.com/500",
            Detail = "An unexpected error occurred while processing the request.",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;

        // Optional in development only
        if (environment.IsDevelopment())
        {
            problemDetails.Extensions["developerMessage"] = exception.ToString();
        }

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private void HandleNonApiException(HttpContext httpContext, string traceId)
    {
        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning("response has already started, aborting redirect");
            return;
        }

        var encodedReturnUrl = Uri.EscapeDataString(httpContext.Request.Path + httpContext.Request.QueryString);
        var location = $"/oops?traceId={Uri.EscapeDataString(traceId)}&returnUrl={encodedReturnUrl}";

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = StatusCodes.Status302Found;
        httpContext.Response.Headers.Location = location;
    }

    private static bool IsApiRequest(PathString path) =>
        path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
}