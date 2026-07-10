namespace Aero.Core.Http;

/// <summary>
/// Represents a class for TenantIdHandler.
/// </summary>
public sealed class TenantIdHandler : DelegatingHandler
{
    private readonly ISiteContext _siteContext;

        /// <summary>
    /// Initializes a new instance of the <see cref="TenantIdHandler"/> class.
    /// </summary>
public TenantIdHandler(ISiteContext siteContext)
    {
        _siteContext = siteContext;
    }

        /// <summary>
    /// SendAsync method.
    /// </summary>
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
