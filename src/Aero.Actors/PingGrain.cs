
using Aero.Actors.Abstractions;
using Aero.Core;
using Orleans.Concurrency;

namespace Aero.Actors;


/// <summary>
/// Represents a class for PingGrain.
/// </summary>
[StatelessWorker]
public class PingGrain(IGrainFactory grainFactory, ILogger<PingGrain> log) 
    : AeroActor(log), IPingGrain
{
        /// <summary>
    /// OnActivateAsync method.
    /// </summary>
public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("PingGrain activated.");
        await base.OnActivateAsync(cancellationToken);
    }

        /// <summary>
    /// OnDeactivateAsync method.
    /// </summary>
public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        log.LogInformation("PingGrain deactivated.");
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

        /// <summary>
    /// Ping method.
    /// </summary>
public async Task<Message> Ping()
    {
        var activity = Span.StartActivity("PingGrain.Ping");
        log.LogInformation("ping received.");
        var id = Snowflake.NewId();

        activity?.SetTag("message.id", id.ToString());

        var pong = grainFactory.GetGrain<IPongGrain>(id, "pong");
        var message = new Message(id, "pong!");

        log.LogInformation("sending pong response");
        return message;
    }
}
