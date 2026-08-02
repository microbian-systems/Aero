namespace Aero.Core.Http;

/// <summary>
/// Represents a class for AeroHttpLoggingHandler.
/// </summary>
public sealed class AeroHttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<AeroHttpLoggingHandler> _logger;

        /// <summary>
    /// Initializes a new instance of the <see cref="AeroHttpLoggingHandler"/> class.
    /// </summary>
public AeroHttpLoggingHandler(ILogger<AeroHttpLoggingHandler> logger)
    {
        _logger = logger;
    }

        /// <summary>
    /// SendAsync method.
    /// </summary>
protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "HTTP request started: {Method} {Uri}",
            request.Method,
            request.RequestUri);

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation(
            "HTTP request completed: {Method} {Uri} {StatusCode}",
            request.Method,
            request.RequestUri,
            response.StatusCode);

        return response;
    }
}
