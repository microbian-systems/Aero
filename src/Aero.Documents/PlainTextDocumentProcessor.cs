using System.Text;

namespace Aero.Documents;

/// <summary>Strict UTF-8/UTF-16/UTF-32 text decoding with BOM support and line anchors.</summary>
public sealed class PlainTextDocumentProcessor : DocumentProcessor
{
    /// <inheritdoc />
    public override string Extension => ".txt";
    /// <inheritdoc />
    protected override ExtractionResult Extract(byte[] content, ExtractionLimits limits, CancellationToken cancellationToken)
    {
        var (encoding, offset) = EncodingFor(content);
        var text = encoding.GetString(content, offset, content.Length - offset);
        if (text.Any(c => c == '\0' || (char.IsControl(c) && c is not '\n' and not '\r' and not '\t')))
            return Failure("InvalidText", "Text contains binary/control characters.");
        var result = new ExtractionBuffer("txt:1", limits);
        using var reader = new StringReader(text);
        int lineNumber = 0;
        int paragraphStart = 1;
        var paragraph = new List<string>();
        while (reader.ReadLine() is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (++lineNumber > limits.MaxUnits)
            {
                result.Issue("UnitLimit", "Line limit reached.");
                break;
            }
            if (string.IsNullOrWhiteSpace(line))
            {
                if (!FlushParagraph(paragraph, paragraphStart, result)) break;
            }
            else
            {
                if (paragraph.Count == 0) paragraphStart = lineNumber;
                paragraph.Add(line);
            }
        }
        FlushParagraph(paragraph, paragraphStart, result);
        return result.Result();
    }

    private static bool FlushParagraph(List<string> paragraph, int start, ExtractionBuffer result)
    {
        if (paragraph.Count == 0) return true;
        bool added = result.Add(string.Join("\n", paragraph), new($"lines:{start}-{start + paragraph.Count - 1}"));
        paragraph.Clear();
        return added;
    }
    private static (Encoding Encoding, int Offset) EncodingFor(byte[] bytes)
    {
        var data = bytes.AsSpan();
        if (data.StartsWith(new byte[] { 0xff, 0xfe, 0, 0 })) return (new UTF32Encoding(false, false, true), 4);
        if (data.StartsWith(new byte[] { 0, 0, 0xfe, 0xff })) return (new UTF32Encoding(true, false, true), 4);
        if (data.StartsWith(new byte[] { 0xff, 0xfe })) return (new UnicodeEncoding(false, false, true), 2);
        if (data.StartsWith(new byte[] { 0xfe, 0xff })) return (new UnicodeEncoding(true, false, true), 2);
        return (new UTF8Encoding(false, true), data.StartsWith(new byte[] { 0xef, 0xbb, 0xbf }) ? 3 : 0);
    }
}
