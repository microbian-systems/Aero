using System.Buffers.Binary;

namespace Aero.Documents.Ocr;

internal static class PngValidation
{
    public static bool IsBounded(RenderedPage page, int maxBytes)
    {
        var bytes = page.Png.AsSpan();
        return bytes.Length >= 33 && bytes.Length <= maxBytes && page.Width > 0 && page.Height > 0
            && (long)page.Width * page.Height <= 64_000_000
            && bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
            && BinaryPrimitives.ReadUInt32BigEndian(bytes[8..12]) == 13
            && bytes[12..16].SequenceEqual("IHDR"u8)
            && BinaryPrimitives.ReadUInt32BigEndian(bytes[16..20]) == page.Width
            && BinaryPrimitives.ReadUInt32BigEndian(bytes[20..24]) == page.Height;
    }
}
