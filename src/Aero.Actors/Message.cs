namespace Aero.Actors;

[Serializable]
[GenerateSerializer]
public record Message(long Id, string content);

[Serializable]
[GenerateSerializer]
public record Message<T>(long Id, string content, T payload);