namespace Aero.Cloudflare;

/// <summary>
/// Represents a class for ExternalIpProviders.
/// </summary>
public static class ExternalIpProviders
{
        /// <summary>
    /// Gets or sets the Providers.
    /// </summary>
public static IEnumerable<string> Providers { get; }

    static ExternalIpProviders()
    {
        Providers = new List<string>
        {
            "https://icanhazip.com/",
            "https://ipecho.net/plain",
            "https://whatismyip.akamai.com",
            "https://tnx.nl/ip"
        };
    }
}