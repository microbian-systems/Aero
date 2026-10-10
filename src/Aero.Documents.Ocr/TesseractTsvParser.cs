using System.Globalization;
using System.Text;

namespace Aero.Documents.Ocr;

/// <summary>Validates Tesseract word TSV, retains coordinates/confidence and groups words by source line.</summary>
public sealed class TesseractTsvParser
{
    /// <summary>Parses bounded TSV; rejects malformed/out-of-image words rather than guessing.</summary>
    public OcrResult Parse(string tsv, int imageWidth, int imageHeight, int maxWords = 20000, int maxTextCharacters = 200000)
    {
        ArgumentNullException.ThrowIfNull(tsv);
        ArgumentOutOfRangeException.ThrowIfLessThan(imageWidth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(imageHeight, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxWords, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxTextCharacters, 1);
        var limits = new TsvLimits(imageWidth, imageHeight, maxWords, maxTextCharacters);
        var words = new List<OcrWord>();
        var text = new StringBuilder();
        var issues = new List<DocumentIssue>();
        OcrResult Result() => new("tesseract-cli-tsv:1", text.ToString(), words.ToArray(), issues.ToArray());
        using var reader = new StringReader(tsv);
        const string header = "level\tpage_num\tblock_num\tpar_num\tline_num\tword_num\tleft\ttop\twidth\theight\tconf\ttext";
        if (reader.ReadLine() != header) return new("tesseract-cli-tsv:1", "", [], [new("InvalidOcrOutput", "Unexpected OCR TSV header.")]);
        OcrWord? previous = null;
        while (reader.ReadLine() is { } row)
        {
            if (!AppendRow(row, limits, words, text, issues, ref previous)) break;
        }
        if (words.Count == 0 && issues.Count == 0) issues.Add(new("NoOcrText", "OCR found no readable words."));
        return Result();
    }

    private readonly record struct TsvLimits(int Width, int Height, int Words, int Characters);

    private static bool AppendRow(string row, TsvLimits limits, List<OcrWord> words,
        StringBuilder text, List<DocumentIssue> issues, ref OcrWord? previous)
    {
        if (row.Length == 0) return true;
        var fields = row.Split('\t', 12);
        if (fields.Length != 12 || !int.TryParse(fields[0], out int level) || level < 1 || level > 5)
        { issues.Add(new("InvalidOcrOutput", "Malformed OCR TSV row.")); return false; }
        if (level != 5) return true;
        var word = ReadWord(fields, limits.Width, limits.Height);
        if (word is null)
        { issues.Add(new("InvalidOcrOutput", "Invalid OCR word geometry, confidence or text.")); return false; }
        if (string.IsNullOrWhiteSpace(word.Text)) return true;
        string separator = Separator(text.Length, previous, word);
        if (words.Count >= limits.Words || word.Text.Length + separator.Length > limits.Characters - text.Length)
        { issues.Add(new("OcrOutputLimit", "OCR word/text budget reached.")); return false; }
        text.Append(separator).Append(word.Text);
        words.Add(word);
        previous = word;
        return true;
    }

    private static OcrWord? ReadWord(string[] fields, int imageWidth, int imageHeight)
    {
        var integers = new int[9];
        bool valid = true;
        for (int n = 0; n < integers.Length; n++)
            valid &= int.TryParse(fields[n + 1], NumberStyles.None, CultureInfo.InvariantCulture, out integers[n]);
        valid &= double.TryParse(fields[10], NumberStyles.Float, CultureInfo.InvariantCulture, out double confidence)
            && double.IsFinite(confidence) && confidence >= 0 && confidence <= 100;
        int left = integers[5], top = integers[6], width = integers[7], height = integers[8];
        valid &= integers[0] == 1 && integers[1] > 0 && integers[2] > 0 && integers[3] > 0 && integers[4] > 0
            && left >= 0 && top >= 0 && width > 0 && height > 0
            && (long)left + width <= imageWidth && (long)top + height <= imageHeight;
        if (!valid || fields[11].Any(char.IsControl)) return null;
        return new(fields[11], left, top, width, height, confidence, integers[1], integers[2], integers[3]);
    }

    private static string Separator(int textLength, OcrWord? previous, OcrWord word)
    {
        if (textLength == 0) return "";
        bool sameLine = previous is not null && previous.Block == word.Block
            && previous.Paragraph == word.Paragraph && previous.Line == word.Line;
        return sameLine ? " " : "\n";
    }
}
