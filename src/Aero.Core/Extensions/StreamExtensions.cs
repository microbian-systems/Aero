using System.IO.Compression;

namespace Aero.Core.Extensions;

/// <summary>
/// Represents a class for StreamExtensions.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// A helper method to return a compressed version of a MemoryStream
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    public static MemoryStream Compress(this MemoryStream ms)
    {
        // Compress
        var compressedMemoryStream = new MemoryStream();
        var gzipStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress);
        gzipStream.Write(ms.ToArray(), 0, (int)ms.Length);
        gzipStream.Close();
        return compressedMemoryStream;
    }

        /// <summary>
    /// LoadStreamWithJson method.
    /// </summary>
public static  MemoryStream LoadStreamWithJson(this MemoryStream ms, string json)
    {
        var sw = new StreamWriter(ms);
        sw.Write(json);
        sw.Flush();
        ms.Position = 0;
        return ms;
    }
        
        /// <summary>
    /// StripTrailingBackSlash method.
    /// </summary>
public static string StripTrailingBackSlash(this string path) => path.TrimEnd('/');
}