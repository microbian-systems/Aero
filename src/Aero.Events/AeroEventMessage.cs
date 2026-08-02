using Aero.Core;
using Wolverine;

namespace Aero.Events;

/// <summary>
/// Defines an interface for IAeroEvent.
/// </summary>
public interface IAeroEvent;

/// <summary>
/// Defines an interface for IAeroEventMessage.
/// </summary>
public interface IAeroEventMessage : IAeroEvent, IMessage
{
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
long Id { get; init; }
        /// <summary>
    /// Gets or sets the Created At.
    /// </summary>
public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Represents a record for AeroEventMessageBase.
/// </summary>
public abstract record AeroEventMessageBase : IAeroEventMessage
{
    // var (timeStamp, machineId, sequence) = sonyflake.DecodeID(uniqueId);
        /// <summary>
    /// Gets or sets the Id.
    /// </summary>
public long Id { get; init; } = Snowflake.NewId();
        /// <summary>
    /// Gets or sets the Created At.
    /// </summary>
public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents a record for AeroEventMessage.
/// </summary>
public abstract record AeroEventMessage : AeroEventMessageBase { }

/// <summary>
/// Represents a record for AeroEventMessage.
/// </summary>
public abstract record AeroEventMessage<T> : AeroEventMessageBase
{
        /// <summary>
    /// Payload.
    /// </summary>
public required T Payload;
}

/// <summary>
/// Represents a record for AeroEvent.
/// </summary>
public abstract record AeroEvent(string message);
