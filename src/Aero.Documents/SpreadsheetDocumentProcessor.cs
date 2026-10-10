using System.Globalization;
using DocumentFormat.OpenXml.Packaging;
using S = DocumentFormat.OpenXml.Spreadsheet;

namespace Aero.Documents;

/// <summary>Read-only Open XML row extraction; retains coordinates, formulas and raw stored values.</summary>
public sealed class SpreadsheetDocumentProcessor : DocumentProcessor
{
    /// <inheritdoc />
    public override string Extension => ".xlsx";
    /// <inheritdoc />
    protected override ExtractionResult Extract(byte[] content, ExtractionLimits limits, CancellationToken cancellationToken)
    {
        CheckOfficePackage(content, limits, "xl/workbook.xml", cancellationToken);
        using var stream = new MemoryStream(content, false);
        using var doc = SpreadsheetDocument.Open(stream, false,
            new OpenSettings { MaxCharactersInPart = limits.MaxExpandedBytes });
        var part = doc.WorkbookPart ?? throw new InvalidDataException();
        var workbook = part.Workbook ?? throw new InvalidDataException();
        var extraction = new WorkbookExtraction(part, limits, cancellationToken);
        foreach (var sheet in workbook.Sheets?.Elements<S.Sheet>() ?? [])
            if (!extraction.ExtractSheet(sheet)) return extraction.Result();
        return extraction.Complete();
    }

    // Per-document counters and lookup tables: never reused between documents or tenants.
    private sealed class WorkbookExtraction
    {
        private readonly WorkbookPart part;
        private readonly ExtractionLimits limits;
        private readonly CancellationToken cancellationToken;
        private readonly ExtractionBuffer result;
        private readonly string[] sharedStrings;
        private readonly S.CellFormat[] formats;
        private readonly Dictionary<uint, string> customFormats;
        private int rowCount;
        private int cellCount;

        public WorkbookExtraction(WorkbookPart part, ExtractionLimits limits, CancellationToken cancellationToken)
        {
            this.part = part;
            this.limits = limits;
            this.cancellationToken = cancellationToken;
            result = new("openxml-xlsx:1", limits);
            sharedStrings = part.SharedStringTablePart?.SharedStringTable?.Elements<S.SharedStringItem>()
                .Select(x => string.Concat(x.Descendants<S.Text>().Select(t => t.Text))).ToArray() ?? [];
            formats = part.WorkbookStylesPart?.Stylesheet?.CellFormats?.Elements<S.CellFormat>().ToArray() ?? [];
            customFormats = part.WorkbookStylesPart?.Stylesheet?.NumberingFormats?.Elements<S.NumberingFormat>()
                .Where(x => x.NumberFormatId is not null)
                .ToDictionary(x => x.NumberFormatId?.Value ?? throw new InvalidDataException(), x => x.FormatCode?.Value ?? "") ?? [];
        }

        public ExtractionResult Result() => result.Result();

        public ExtractionResult Complete()
        {
            result.Issue("SpreadsheetCoverage", "Raw stored values and styles only; visual formatting, merged-cell meaning, drawings and external links are not interpreted.");
            return Result();
        }

        public bool ExtractSheet(S.Sheet sheet)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string name = sheet.Name?.Value ?? "unnamed";
            var worksheet = part.GetPartById(sheet.Id?.Value ?? throw new InvalidDataException()) as WorksheetPart
                ?? throw new InvalidDataException();
            string context = SheetContext(sheet, name);
            string initialRow = "";
            var sheetData = worksheet.Worksheet?.GetFirstChild<S.SheetData>();
            if (sheetData is null)
            {
                result.Issue("MissingSheetData", "Worksheet has no sheet data.", new("worksheet", Sheet: name));
                return true;
            }
            foreach (var row in sheetData.Elements<S.Row>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var location = new DocumentLocation("worksheet", Sheet: name, Row: row.RowIndex?.Value);
                if (++rowCount > limits.MaxUnits)
                {
                    result.Issue("UnitLimit", "Workbook row limit reached.", location);
                    return false;
                }
                if (!ExtractRow(row, location, context, out string text)) return false;
                if (text.Length == 0) continue;
                // Carry actual preceding content, not a guessed first-row header.
                if (initialRow.Length == 0) initialRow = Preview(text);
                context = $"{SheetContext(sheet, name)}; initial row (not inferred headers): {initialRow}; preceding row: {Preview(text)}";
            }
            return true;
        }

        private bool ExtractRow(S.Row row, DocumentLocation location, string context, out string text)
        {
            var cells = new List<string>();
            string? firstAddress = null, lastAddress = null;
            text = "";
            foreach (var cell in row.Elements<S.Cell>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (++cellCount > limits.MaxCells)
                {
                    result.Issue("CellLimit", "Workbook cell limit reached; the incomplete row was not retained.", location);
                    return false;
                }
                var address = cell.CellReference?.Value ?? "unknown";
                firstAddress ??= address;
                lastAddress = address;
                cells.Add(DescribeCell(cell, location with { CellRange = address }, address));
            }
            if (cells.Count == 0) return true;
            text = string.Join(" | ", cells);
            return result.Add(text, location with { CellRange = $"{firstAddress}:{lastAddress}" }, context);
        }

        private string DescribeCell(S.Cell cell, DocumentLocation location, string address)
        {
            string value = CellValue(cell, location);
            var type = cell.DataType?.Value.ToString() ?? "NumberOrDateSerial";
            string formula = DescribeFormula(cell, location);
            if (cell.DataType?.Value == S.CellValues.Error)
                result.Issue("CellError", "A worksheet contains a stored Excel error.", location);
            return $"{address}: value={value}; type={type}; {DescribeStyle(cell)}{formula}";
        }

        private string CellValue(S.Cell cell, DocumentLocation location)
        {
            var raw = cell.CellValue?.Text ?? "";
            if (cell.DataType?.Value == S.CellValues.SharedString) return SharedValue(raw, location);
            if (cell.DataType?.Value == S.CellValues.InlineString)
                return string.Concat(cell.InlineString?.Descendants<S.Text>().Select(x => x.Text) ?? []);
            if (cell.DataType?.Value != S.CellValues.Boolean) return raw;
            if (raw is not "0" and not "1") result.Issue("InvalidBoolean", "Invalid stored Boolean value.", location);
            return raw switch { "0" => "false", "1" => "true", _ => raw };
        }

        private string SharedValue(string raw, DocumentLocation location)
        {
            if (int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out int index)
                && index >= 0 && index < sharedStrings.Length) return sharedStrings[index];
            result.Issue("InvalidSharedString", "A shared-string reference could not be resolved.", location);
            return "[invalid shared-string index]";
        }

        private string DescribeFormula(S.Cell cell, DocumentLocation location)
        {
            var formula = cell.CellFormula;
            if (formula is null) return "";
            result.Issue("CachedFormulaValues", "Formulas are not evaluated; cached results may be absent or stale.");
            if (cell.CellValue is null) result.Issue("MissingFormulaResult", "Formula has no cached result.", location);
            return $"; formula={formula.Text}; formulaType={formula.FormulaType?.Value.ToString() ?? "Normal"}; sharedIndex={formula.SharedIndex?.Value.ToString(CultureInfo.InvariantCulture) ?? "none"}";
        }

        private string DescribeStyle(S.Cell cell)
        {
            uint index = cell.StyleIndex?.Value ?? 0;
            string style = $"styleIndex={index}";
            if (index < formats.Length && formats[index].NumberFormatId?.Value is { } id)
                style += $"; numberFormatId={id}; formatCode={customFormats.GetValueOrDefault(id, "[built-in]")}";
            return style;
        }

        private static string SheetContext(S.Sheet sheet, string name) =>
            $"Worksheet: {name}; state: {sheet.State?.Value.ToString() ?? "Visible"}";
        private static string Preview(string text) => text[..Math.Min(text.Length, 1024)];
    }
}
