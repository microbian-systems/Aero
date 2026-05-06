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

/// <summary>
/// A seed-specific implementation of ISiteContext that returns a fixed site and tenant ID.
/// Used during database seeding when there is no HTTP request context available.
/// </summary>
public sealed class SeedSiteContext(long siteId, long tenantId = 0) : ISiteContext
{
    public long SiteId { get; } = siteId;
    public long TenantId { get; } = tenantId;
}
