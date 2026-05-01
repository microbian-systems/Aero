namespace Aero.Core.Http;

public sealed class ClientRateLimitHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim Semaphore = new(100);

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
