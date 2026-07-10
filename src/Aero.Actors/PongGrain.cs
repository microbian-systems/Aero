using Aero.Actors.Abstractions;
using Orleans.Concurrency;

namespace Aero.Actors;

/// <summary>
/// Defines an interface for IPongGrain.
/// </summary>
public interface IPongGrain : IAeroActor
{
        /// <summary>
    /// AreYouAwake method.
    /// </summary>
Task<string> AreYouAwake(Message message);
}

/// <summary>
/// Represents a class for PongGrain.
/// </summary>
[StatelessWorker]
public class PongGrain(ILogger<PongGrain> log) : AeroActor(log), IPongGrain
{
        /// <summary>
    /// AreYouAwake method.
    /// </summary>
public Task<string> AreYouAwake(Message message)
    {
        var activity = Span.StartActivity("PongGrain.AreYouAwake");
        log.LogInformation("Ping received: {Content}", message.content);
        activity?.SetTag("message.id", message.Id.ToString());
        return Task.FromResult($"pong! ping received: {message.content}");
    }
}