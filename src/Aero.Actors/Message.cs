namespace Aero.Actors;

[Serializable]
[GenerateSerializer]
public record Message(ulong Id, string content);

[Serializable]
[GenerateSerializer]
public record Message<T>(ulong Id, string content, T payload);