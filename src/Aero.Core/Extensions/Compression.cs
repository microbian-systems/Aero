using System.IO.Compression;

namespace Aero.Core.Extensions;

// todo - create unit tests for Aero.Core.CompressionHelpers
/// <summary>
/// Represents a class for Compression.
/// </summary>
public static class Compression
{
        /// <summary>
    /// Compress method.
    /// </summary>
public static byte[] Compress(byte[] data)
    {
        byte[] compressArray = null;

        using (var memoryStream = new MemoryStream())
        using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
        {
            deflateStream.Write(data, 0, data.Length);
            compressArray = memoryStream.ToArray();
        }

        return compressArray;
    }

        /// <summary>
    /// Decompress method.
    /// </summary>
public static byte[] Decompress(byte[] data)
    {
        byte[] decompressedArray = null;

        using (var decompressedStream = new MemoryStream())
        using (var compressStream = new MemoryStream(data))
        using (var deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
        {
            deflateStream.CopyTo(decompressedStream);
            decompressedArray = decompressedStream.ToArray();
        }

        return decompressedArray;
    }
}