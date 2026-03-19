using Aero.Core;
using Wolverine;

namespace Aero.Events;

public interface IEvent
{

}

public interface IEventMessage : IEvent, IMessage
{
    ulong Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public abstract record EventMessageBase : IEventMessage
{
    // var (timeStamp, machineId, sequence) = sonyflake.DecodeID(uniqueId);
    public ulong Id { get; init; } = Snowflake.NewId();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public abstract record EventMessage : EventMessageBase { }

public abstract record EventMessage<T> : EventMessageBase
{
    public required T Payload;
}

