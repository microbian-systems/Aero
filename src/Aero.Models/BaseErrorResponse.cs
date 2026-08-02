namespace Aero.Models;

// todo - rename BaseErrorResponse -> Just Error or AppError
/// <summary>
/// Represents a class for BaseErrorResponse.
/// </summary>
public class BaseErrorResponse
{
    /// <summary>
    /// Error code
    /// </summary>
    [JsonPropertyName("status_code")]
    public string StatusCode { get; set; } = 200.ToString();

    /// <summary>
    /// Field which has the error
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; }

        /// <summary>
    /// Gets or sets the Details.
    /// </summary>
[JsonPropertyName("details")]
    public string Details { get; set; }
}