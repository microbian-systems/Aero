namespace Aero.Actors.Abstractions;

/// <summary>
/// Defines an interface for IAeroActor.
/// </summary>
public interface IAeroActor : IGrainWithIntegerKey;

/// <summary>
/// Defines an interface for IPingGrain.
/// </summary>
public interface IPingGrain : IAeroActor
{
        /// <summary>
    /// Ping method.
    /// </summary>
Task<Message> Ping();
}
