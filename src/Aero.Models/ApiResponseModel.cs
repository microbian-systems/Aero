using System.Net;

namespace Aero.Models;


/// <summary>
/// Represents a record for WebResponseDynamicModel.
/// </summary>
public record WebResponseDynamicModel : WebResponseModel<dynamic>
{
}

/// <summary>
/// Represents a record for WebResponseObjectModel.
/// </summary>
public record WebResponseObjectModel : WebResponseModel<object>
{
}

/// <summary>
/// Represents a record for WebResponseCollectionModel.
/// </summary>
public record WebResponseCollectionModel<T> : WebResponseModel<List<T>>, IWebResponseCollectionModel<T>
{
}

/// <summary>
/// Represents a record for WebResponseModel.
/// </summary>
public record WebResponseModel<T> : WebResponseModel, IWebResponseModel<T>
{
        /// <summary>
    /// Gets or sets the Data.
    /// </summary>
public virtual T Data { get; set; } = default!;
}

/// <summary>
/// Represents a record for WebResponseModel.
/// </summary>
public record WebResponseModel : IWebResponseModel
{
        /// <summary>
    /// Gets or sets the Status Code.
    /// </summary>
public HttpStatusCode StatusCode { get; set; }
        /// <summary>
    /// Gets or sets the Reason Phrase.
    /// </summary>
public string ReasonPhrase { get; set; } = string.Empty;
        /// <summary>
    /// Gets or sets the Is Success Status Code.
    /// </summary>
public bool IsSuccessStatusCode { get; set; }
}
