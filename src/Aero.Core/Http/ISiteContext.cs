namespace Aero.Core.Http;

/// <summary>
/// Defines an interface for ISiteContext.
/// </summary>
public interface ISiteContext
{
        /// <summary>
    /// Gets or sets the Site Id.
    /// </summary>
long SiteId { get; }
        /// <summary>
    /// Gets or sets the Tenant Id.
    /// </summary>
long TenantId { get; }
}
