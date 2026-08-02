namespace Aero.Web.Models;

/// <summary>
/// Represents a record for RequestResponseLogModel.
/// </summary>
public record RequestResponseLogModel()
{
        /// <summary>
    /// Gets or sets the Scheme.
    /// </summary>
public string Scheme { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Host.
    /// </summary>
public string Host { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Path.
    /// </summary>
public string Path { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Query String.
    /// </summary>
public string QueryString { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Method.
    /// </summary>
public string Method { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Request Body.
    /// </summary>
public string RequestBody { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Response Body.
    /// </summary>
public string ResponseBody { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Response Content Type.
    /// </summary>
public string ResponseContentType { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Response Status Code.
    /// </summary>
public int ResponseStatusCode { get; set; }
        /// <summary>
    /// Gets or sets the Response Status Message.
    /// </summary>
public string ResponseStatusMessage { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Response Status Description.
    /// </summary>
public string ResponseStatusDescription { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Response Headers.
    /// </summary>
public Dictionary<string, string> ResponseHeaders { get; set; } = new();
        /// <summary>
    /// Gets or sets the Request Headers.
    /// </summary>
public Dictionary<string, string> RequestHeaders { get; set; } = new();
        /// <summary>
    /// Gets or sets the Request Protocol.
    /// </summary>
public string RequestProtocol { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Request Remote Ip Address.
    /// </summary>
public string RequestRemoteIpAddress { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Request Remote Port.
    /// </summary>
public string RequestRemotePort { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Request Local Ip Address.
    /// </summary>
public string RequestLocalIpAddress { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Request Local Port.
    /// </summary>
public string RequestLocalPort { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Request Id.
    /// </summary>
public string RequestId { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Request Trace Identifier.
    /// </summary>
public string RequestTraceIdentifier { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Request Is Https.
    /// </summary>
public bool RequestIsHttps { get; set; }
        /// <summary>
    /// Gets or sets the Request Is Web Socket Request.
    /// </summary>
public bool RequestIsWebSocketRequest { get; set; }
        /// <summary>
    /// Gets or sets the Request Is Secure Connection.
    /// </summary>
public bool RequestIsSecureConnection { get; set; }
        /// <summary>
    /// Gets or sets the Request Is Local.
    /// </summary>
public bool RequestIsLocal { get; set; }
        /// <summary>
    /// Gets or sets the Request Is Authenticated.
    /// </summary>
public bool RequestIsAuthenticated { get; set; }
}