namespace Aero.Core.Http;

public sealed class CorrelationIdHandler : DelegatingHandler
{
    private readonly ICorrelationIdAccessor _accessor;

    public CorrelationIdHandler(ICorrelationIdAccessor accessor)
    {
        _accessor = accessor;
    }

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
