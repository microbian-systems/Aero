
using Aero.Actors.Abstractions;
using Aero.Core;
using Orleans.Concurrency;

namespace Aero.Actors;


[StatelessWorker]
public class PingGrain(ILogger<PingGrain> log, IGrainFactory grainFactory) : AeroActor(log), IPingGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("PingGrain activated.");
        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        log.LogInformation("PingGrain deactivated.");
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    public async Task<Message> Ping()
    {
        var activity = Span.StartActivity("PingGrain.Ping");
        log.LogInformation("ping received.");
        var id = Snowflake.NewId();

        activity?.SetTag("message.id", id.ToString());

        var pong = grainFactory.GetGrain<IPongGrain>(id);
        var message = new Message(id, "pong!");

        log.LogInformation("sending pong response");
        return message;
    }
}
