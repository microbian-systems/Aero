using Aero.Core;
using Wolverine;

namespace Aero.Events;

public interface IAeroEvent;

public interface IAeroEventMessage : IAeroEvent, IMessage
{
    long Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public abstract record AeroEventMessageBase : IAeroEventMessage
{
    // var (timeStamp, machineId, sequence) = sonyflake.DecodeID(uniqueId);
    public long Id { get; init; } = Snowflake.NewId();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public abstract record AeroEventMessage : AeroEventMessageBase { }

public abstract record AeroEventMessage<T> : AeroEventMessageBase
{
    public required T Payload;
}

