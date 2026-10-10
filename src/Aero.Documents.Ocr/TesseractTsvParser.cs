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
        var words = new List<OcrWord>();
        var text = new StringBuilder();
        var issues = new List<DocumentIssue>();
        OcrResult Result() => new("tesseract-cli-tsv:1", text.ToString(), words.ToArray(), issues.ToArray());
        using var reader = new StringReader(tsv);
        const string header = "level\tpage_num\tblock_num\tpar_num\tline_num\tword_num\tleft\ttop\twidth\theight\tconf\ttext";
        if (reader.ReadLine() != header) return new("tesseract-cli-tsv:1", "", [], [new("InvalidOcrOutput", "Unexpected OCR TSV header.")]);
        (int Block, int Paragraph, int Line)? previous = null;
        while (reader.ReadLine() is { } row)
        {
            if (row.Length == 0) continue;
            var fields = row.Split('\t', 12);
            if (fields.Length != 12 || !int.TryParse(fields[0], out int level) || level < 1 || level > 5)
            { issues.Add(new("InvalidOcrOutput", "Malformed OCR TSV row.")); break; }
            if (level != 5) continue;
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
            string value = fields[11];
            if (!valid || value.Any(char.IsControl))
            { issues.Add(new("InvalidOcrOutput", "Invalid OCR word geometry, confidence or text.")); break; }
            if (string.IsNullOrWhiteSpace(value)) continue;
            var line = (integers[1], integers[2], integers[3]);
            string separator = text.Length == 0 ? "" : previous == line ? " " : "\n";
            if (words.Count >= maxWords || value.Length + separator.Length > maxTextCharacters - text.Length)
            { issues.Add(new("OcrOutputLimit", "OCR word/text budget reached.")); break; }
            text.Append(separator).Append(value);
            words.Add(new(value, left, top, width, height, confidence, line.Item1, line.Item2, line.Item3));
            previous = line;
        }
        if (words.Count == 0 && issues.Count == 0) issues.Add(new("NoOcrText", "OCR found no readable words."));
        return Result();
    }
}
