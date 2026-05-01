using Microsoft.Extensions.Logging;

namespace Aero.Core.Http;

public sealed class AeroHttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<AeroHttpLoggingHandler> _logger;

    public AeroHttpLoggingHandler(ILogger<AeroHttpLoggingHandler> logger)
    {
        _logger = logger;
    }

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
