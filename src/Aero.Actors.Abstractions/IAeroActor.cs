namespace Aero.Actors.Abstractions;

public interface IAeroActor : IGrainWithIntegerKey;

public interface IPingGrain : IAeroActor
{
    Task<Message> Ping();
}
