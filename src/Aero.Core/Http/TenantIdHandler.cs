namespace Aero.Core.Http;

public sealed class TenantIdHandler : DelegatingHandler
{
    private readonly ISiteContext _siteContext;

    public TenantIdHandler(ISiteContext siteContext)
    {
        _siteContext = siteContext;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.TryAddWithoutValidation(
            "X-Tenant-Id",
            _siteContext.TenantId.ToString());

        request.Headers.TryAddWithoutValidation(
            "X-Site-Id",
            _siteContext.SiteId.ToString());

        return base.SendAsync(request, cancellationToken);
    }
}
