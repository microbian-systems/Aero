using System.Net;

namespace Aero.Models;


/// <summary>
/// Defines an interface for IWebResponseModel.
/// </summary>
public interface IWebResponseModel 
{
        /// <summary>
    /// Gets or sets the Status Code.
    /// </summary>
HttpStatusCode StatusCode { get; set; }
        /// <summary>
    /// Gets or sets the Reason Phrase.
    /// </summary>
string ReasonPhrase { get; set; }
        /// <summary>
    /// Gets or sets the Is Success Status Code.
    /// </summary>
bool IsSuccessStatusCode { get; set; }
}

/// <summary>
/// Defines an interface for IWebResponseModel.
/// </summary>
public interface IWebResponseModel<T> : IWebResponseModel
{
        /// <summary>
    /// Gets or sets the Data.
    /// </summary>
T Data { get; set; }
}

/// <summary>
/// Defines an interface for IWebResponseCollectionModel.
/// </summary>
public interface IWebResponseCollectionModel<T> : IWebResponseModel<List<T>>
{
}
