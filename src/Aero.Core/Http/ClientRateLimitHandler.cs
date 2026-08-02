namespace Aero.Core.Http;

/// <summary>
/// Represents a class for ClientRateLimitHandler.
/// </summary>
public sealed class ClientRateLimitHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim Semaphore = new(100);

        /// <summary>
    /// SendAsync method.
    /// </summary>
protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await Semaphore.WaitAsync(cancellationToken);

        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
