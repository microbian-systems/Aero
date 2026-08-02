using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Aero.Core.Workers;

/// <summary>
/// Represents a class for BackgroundServiceBase.
/// </summary>
public abstract class BackgroundServiceBase(
    IServiceProvider sp,
    ILogger<BackgroundServiceBase> log,
    IConfiguration config)
    : BackgroundService
{

        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
protected abstract override Task ExecuteAsync(CancellationToken stoppingToken);
}