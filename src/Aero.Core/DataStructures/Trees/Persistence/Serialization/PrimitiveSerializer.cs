using System.Runtime.InteropServices;

namespace Aero.Core.DataStructures.Trees.Persistence.Serialization;

/// <summary>
/// Represents a class for PrimitiveSerializer.
/// </summary>
public sealed class PrimitiveSerializer<T> : INodeSerializer<T> where T : unmanaged
{
        /// <summary>
    /// Gets or sets the Serialized Size.
    /// </summary>
public int SerializedSize => Marshal.SizeOf<T>();

        /// <summary>
    /// Deserialize method.
    /// </summary>
public T Deserialize(ReadOnlySpan<byte> data)
    {
        if (data.Length < SerializedSize)
            throw new ArgumentException($"Data span too small. Expected {SerializedSize} bytes.", nameof(data));
        
        return MemoryMarshal.Read<T>(data);
    }

        /// <summary>
    /// Serialize method.
    /// </summary>
public void Serialize(T node, Span<byte> destination)
    {
        if (destination.Length < SerializedSize)
            throw new ArgumentException($"Destination span too small. Expected {SerializedSize} bytes.", nameof(destination));
        
        MemoryMarshal.Write(destination, ref node);
    }
}
