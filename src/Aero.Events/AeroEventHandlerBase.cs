using Microsoft.Extensions.Logging;

namespace Aero.Events;

public interface IAeroEventHandlerBase { }

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