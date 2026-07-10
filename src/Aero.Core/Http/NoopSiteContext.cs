namespace Aero.Core.Http;

/// <summary>
/// A no-op implementation of ISiteContext that returns 0 for all IDs.
/// Suitable for environments without request context like WASM or MAUI.
/// </summary>
public sealed class NoopSiteContext : ISiteContext
{
        /// <summary>
    /// Gets or sets the Site Id.
    /// </summary>
public long SiteId => 0;
        /// <summary>
    /// Gets or sets the Tenant Id.
    /// </summary>
public long TenantId => 0;
}

/// <summary>
/// A seed-specific implementation of ISiteContext that returns a fixed site and tenant ID.
/// Used during database seeding when there is no HTTP request context available.
/// </summary>
public sealed class SeedSiteContext(long siteId, long tenantId = 0) : ISiteContext
{
        /// <summary>
    /// Gets or sets the Site Id.
    /// </summary>
public long SiteId { get; } = siteId;
        /// <summary>
    /// Gets or sets the Tenant Id.
    /// </summary>
public long TenantId { get; } = tenantId;
}
