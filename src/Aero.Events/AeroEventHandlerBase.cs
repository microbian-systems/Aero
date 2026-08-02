using Microsoft.Extensions.Logging;

namespace Aero.Events;

/// <summary>
/// Defines an interface for IAeroEventHandlerBase.
/// </summary>
public interface IAeroEventHandlerBase { }

/// <summary>
/// Represents a class for AeroEventHandlerBase.
/// </summary>
public abstract class AeroEventHandlerBase(ILogger<AeroEventHandlerBase> log) : IAeroEventHandlerBase
{
    /// <summary>
    /// Cancellation token support for event handlers
    /// </summary>
    /// <param name="timeout">the timeout in minutes</param>
    /// <returns><see cref="CancellationToken"/></returns>
    protected CancellationToken GetToken(int timeout = 10) 
        => new CancellationTokenSource(TimeSpan.FromMinutes(timeout)).Token;
};