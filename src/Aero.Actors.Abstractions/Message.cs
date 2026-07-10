namespace Aero.Actors.Abstractions;

/// <summary>
/// Represents a record for Message.
/// </summary>
[GenerateSerializer]
public record Message(
    [property: Id(0)] long Id,
    [property: Id(1)] string content);

/// <summary>
/// Represents a record for Message.
/// </summary>
[GenerateSerializer]
public record Message<T>(
    [property: Id(0)] long Id,
    [property: Id(1)] string content,
    [property: Id(2)] T payload);