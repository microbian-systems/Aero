namespace Aero.Actors.Abstractions;

[GenerateSerializer]
public record Message(
    [property: Id(0)] long Id,
    [property: Id(1)] string content);

[GenerateSerializer]
public record Message<T>(
    [property: Id(0)] long Id,
    [property: Id(1)] string content,
    [property: Id(2)] T payload);