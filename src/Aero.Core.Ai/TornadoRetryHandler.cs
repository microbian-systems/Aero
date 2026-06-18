using Microsoft.Extensions.Logging;

namespace Aero.Core.Ai;

/// <summary>
/// DelegatingHandler that retries outbound LLM requests only on transient
/// connection failures and timeouts. Does NOT retry on HTTP error responses
/// (4xx, 5xx) — those are API-level failures that are expensive to repeat.
/// </summary>
public sealed class TornadoRetryHandler(ILogger<TornadoRetryHandler> logger) : DelegatingHandler
{
    private const int MaxRetries = 2;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                // Got a response (any status) — do NOT retry, return immediately
                return response;
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries)
            {
                logger.LogWarning(ex,
                    "LLM connection failure (attempt {Attempt}/{Max}). Retrying in {Delay}ms...",
                    attempt + 1, MaxRetries + 1, RetryDelay.TotalMilliseconds);
                await Task.Delay(RetryDelay, cancellationToken);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested && attempt < MaxRetries)
            {
                logger.LogWarning(ex,
                    "LLM request timed out (attempt {Attempt}/{Max}). Retrying in {Delay}ms...",
                    attempt + 1, MaxRetries + 1, RetryDelay.TotalMilliseconds);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        // Final attempt — let any exception propagate
        return await base.SendAsync(request, cancellationToken);
    }
}
