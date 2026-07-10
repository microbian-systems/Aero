namespace Aero.Core.DataStructures.Trees.Persistence.Serialization;

/// <summary>
/// Defines an interface for IDocumentSerializer.
/// </summary>
public interface IDocumentSerializer<TDocument>
    where TDocument : class
{
        /// <summary>
    /// Serialize method.
    /// </summary>
ReadOnlyMemory<byte> Serialize(TDocument document);
        /// <summary>
    /// Deserialize method.
    /// </summary>
TDocument Deserialize(ReadOnlyMemory<byte> bytes);
}

/// <summary>
/// Represents a class for SerializationException.
/// </summary>
public sealed class SerializationException : Exception
{
        /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class.
    /// </summary>
public SerializationException(string message) : base(message) { }
        /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class.
    /// </summary>
public SerializationException(string message, Exception inner) : base(message, inner) { }
}
