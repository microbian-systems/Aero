namespace Aero.Core.Http;

/// <summary>
/// Represents a class for CorrelationIdHandler.
/// </summary>
public sealed class CorrelationIdHandler : DelegatingHandler
{
    private readonly ICorrelationIdAccessor _accessor;

        /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdHandler"/> class.
    /// </summary>
public CorrelationIdHandler(ICorrelationIdAccessor accessor)
    {
        _accessor = accessor;
    }

        /// <summary>
    /// SendAsync method.
    /// </summary>
protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = _accessor.CorrelationId;

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.TryAddWithoutValidation(
                "X-Correlation-Id",
                correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
