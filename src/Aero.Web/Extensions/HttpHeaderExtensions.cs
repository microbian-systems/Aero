namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for HttpHeaderExtensions.
/// </summary>
public static class HttpHeaderExtensions
{
        /// <summary>
    /// RemoveHeaders method.
    /// </summary>
public static WebApplicationBuilder RemoveHeaders(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

        return builder;
    }
}