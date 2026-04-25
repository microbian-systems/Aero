namespace Aero.Core.Http;

public interface ISiteContext
{
    long SiteId { get; }
    long TenantId { get; }
}
