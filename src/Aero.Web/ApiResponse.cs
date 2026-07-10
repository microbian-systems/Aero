namespace Aero.Web;


/// <summary>
/// Defines an interface for IApiResponse.
/// </summary>
public interface IApiResponse
{
        /// <summary>
    /// Gets or sets the Status Code.
    /// </summary>
HttpStatusCode StatusCode { get; set; }
        /// <summary>
    /// Gets or sets the Message.
    /// </summary>
string? Message { get; set; }
}

/// <summary>
/// Defines an interface for IApiAuthResponse.
/// </summary>
public interface IApiAuthResponse<T> : IApiResponse
{
        /// <summary>
    /// Gets or sets the Data.
    /// </summary>
T Data { get; set; }
}

/// <summary>
/// Represents a class for ApiAuthResponse.
/// </summary>
public class ApiAuthResponse : IApiResponse
{
        /// <summary>
    /// Gets or sets the Status Code.
    /// </summary>
[JsonPropertyName("statusCode")]
    public HttpStatusCode StatusCode { get; set; }
        /// <summary>
    /// Gets or sets the Message.
    /// </summary>
[JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// Represents a class for ApiAuthResponse.
/// </summary>
public class ApiAuthResponse<T> : ApiAuthResponse, IApiAuthResponse<T>
{
        /// <summary>
    /// Gets or sets the Data.
    /// </summary>
[JsonPropertyName("data")]
    public T Data { get; set; }
}