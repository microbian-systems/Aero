namespace Aero.Core.Commands;

/// <summary>
/// Generally useful as/in parameter to methods
/// </summary>
/// <remarks>compatible with orleans serialization</remarks>
public interface IRequest;

/// <summary>
/// Generally useful as/in paramters to methods
/// </summary>
/// <typeparam name="T">The type of data payload</typeparam>
/// <remarks>compatible with orleans serialization</remarks>
public interface IRequest<T>
{
        /// <summary>
    /// Gets or sets the Data.
    /// </summary>
T Data { get; set; }
}