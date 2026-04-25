namespace Aero.Core.Http;

/// <summary>
/// A no-op implementation of ISiteContext that returns 0 for all IDs.
/// Suitable for environments without request context like WASM or MAUI.
/// </summary>
public sealed class NoopSiteContext : ISiteContext
{
    public long SiteId => 0;
    public long TenantId => 0;
}
