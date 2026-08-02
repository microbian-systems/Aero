using System.Buffers;

namespace Aero.Core.DataStructures.Trees.Persistence.Serialization;

/// <summary>
/// Represents a class for SystemTextJsonSerializer.
/// </summary>
public sealed class SystemTextJsonSerializer<TDocument>(JsonSerializerOptions? options = null)
    : IDocumentSerializer<TDocument>
    where TDocument : class
{
    private readonly JsonSerializerOptions _options = options ?? new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

        /// <summary>
    /// Serialize method.
    /// </summary>
public ReadOnlyMemory<byte> Serialize(TDocument document)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        JsonSerializer.Serialize(writer, document, _options);
        writer.Flush();
        return buffer.WrittenMemory;
    }

        /// <summary>
    /// Deserialize method.
    /// </summary>
public TDocument Deserialize(ReadOnlyMemory<byte> bytes)
    {
        var result = JsonSerializer.Deserialize<TDocument>(bytes.Span, _options);
        return result ?? throw new SerializationException(
            $"Deserialization of {typeof(TDocument).Name} returned null.");
    }
}
