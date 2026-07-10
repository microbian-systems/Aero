namespace Aero.Core.Http;

/// <summary>
/// Defines an interface for ICorrelationIdAccessor.
/// </summary>
public interface ICorrelationIdAccessor
{
        /// <summary>
    /// Gets or sets the Correlation Id.
    /// </summary>
string? CorrelationId { get; }
}
