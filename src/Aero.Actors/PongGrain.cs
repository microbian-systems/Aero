using Aero.Actors.Abstractions;
using Orleans.Concurrency;

namespace Aero.Actors;

public interface IPongGrain : IAeroActor
{
    Task<string> AreYouAwake(Message message);
}

[StatelessWorker]
public class PongGrain(ILogger<PongGrain> log) : AeroActor(log), IPongGrain
{
    public Task<string> AreYouAwake(Message message)
    {
        var activity = Span.StartActivity("PongGrain.AreYouAwake");
        log.LogInformation("Ping received: {Content}", message.content);
        activity?.SetTag("message.id", message.Id.ToString());
        return Task.FromResult($"pong! ping received: {message.content}");
    }
}