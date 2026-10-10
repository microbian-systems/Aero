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
        var result = new ExtractionBuffer("openxml-xlsx:1", limits);
        var sharedStrings = part.SharedStringTablePart?.SharedStringTable?.Elements<S.SharedStringItem>()
            .Select(x => string.Concat(x.Descendants<S.Text>().Select(t => t.Text))).ToArray() ?? [];
        var formats = part.WorkbookStylesPart?.Stylesheet?.CellFormats?.Elements<S.CellFormat>().ToArray() ?? [];
        var customFormats = part.WorkbookStylesPart?.Stylesheet?.NumberingFormats?.Elements<S.NumberingFormat>()
            .Where(x => x.NumberFormatId is not null).ToDictionary(x => x.NumberFormatId!.Value, x => x.FormatCode?.Value ?? "") ?? [];
        int rowCount = 0, cellCount = 0;
        foreach (var sheet in workbook.Sheets?.Elements<S.Sheet>() ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            string name = sheet.Name?.Value ?? "unnamed";
            var worksheet = part.GetPartById(sheet.Id?.Value ?? throw new InvalidDataException()) as WorksheetPart
                ?? throw new InvalidDataException();
            string context = $"Worksheet: {name}; state: {sheet.State?.Value.ToString() ?? "Visible"}";
            string initialRow = "";
            var sheetData = worksheet.Worksheet?.GetFirstChild<S.SheetData>();
            if (sheetData is null) { result.Issue("MissingSheetData", "Worksheet has no sheet data.", new("worksheet", Sheet: name)); continue; }
            foreach (var row in sheetData.Elements<S.Row>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rowLocation = new DocumentLocation("worksheet", Sheet: name, Row: row.RowIndex?.Value);
                if (++rowCount > limits.MaxUnits)
                {
                    result.Issue("UnitLimit", "Workbook row limit reached.", rowLocation);
                    return result.Result();
                }
                var cells = new List<string>();
                string? firstAddress = null, lastAddress = null;
                foreach (var cell in row.Elements<S.Cell>())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (++cellCount > limits.MaxCells)
                    {
                        result.Issue("CellLimit", "Workbook cell limit reached; the incomplete row was not retained.", rowLocation);
                        return result.Result();
                    }
                    var address = cell.CellReference?.Value ?? "unknown";
                    firstAddress ??= address;
                    lastAddress = address;
                    var location = rowLocation with { CellRange = address };
                    var raw = cell.CellValue?.Text ?? "";
                    string value = raw;
                    if (cell.DataType?.Value == S.CellValues.SharedString)
                    {
                        if (int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out int index)
                            && index >= 0 && index < sharedStrings.Length) value = sharedStrings[index];
                        else { value = "[invalid shared-string index]"; result.Issue("InvalidSharedString", "A shared-string reference could not be resolved.", location); }
                    }
                    else if (cell.DataType?.Value == S.CellValues.InlineString)
                        value = string.Concat(cell.InlineString?.Descendants<S.Text>().Select(x => x.Text) ?? []);
                    else if (cell.DataType?.Value == S.CellValues.Boolean)
                    {
                        value = raw switch { "0" => "false", "1" => "true", _ => raw };
                        if (raw is not "0" and not "1") result.Issue("InvalidBoolean", "Invalid stored Boolean value.", location);
                    }
                    var type = cell.DataType?.Value.ToString() ?? "NumberOrDateSerial";
                    var formula = cell.CellFormula;
                    string formulaText = formula is null ? "" : $"; formula={formula.Text}; formulaType={formula.FormulaType?.Value.ToString() ?? "Normal"}; sharedIndex={formula.SharedIndex?.Value.ToString(CultureInfo.InvariantCulture) ?? "none"}";
                    if (formula is not null)
                    {
                        result.Issue("CachedFormulaValues", "Formulas are not evaluated; cached results may be absent or stale.");
                        if (cell.CellValue is null) result.Issue("MissingFormulaResult", "Formula has no cached result.", location);
                    }
                    if (cell.DataType?.Value == S.CellValues.Error)
                        result.Issue("CellError", "A worksheet contains a stored Excel error.", location);
                    uint styleIndex = cell.StyleIndex?.Value ?? 0;
                    string style = $"styleIndex={styleIndex}";
                    if (styleIndex < formats.Length && formats[styleIndex].NumberFormatId?.Value is { } formatId)
                        style += $"; numberFormatId={formatId}; formatCode={customFormats.GetValueOrDefault(formatId, "[built-in]")}";
                    cells.Add($"{address}: value={value}; type={type}; {style}{formulaText}");
                }
                if (cells.Count == 0) continue;
                var text = string.Join(" | ", cells);
                if (!result.Add(text, rowLocation with { CellRange = $"{firstAddress}:{lastAddress}" }, context)) return result.Result();
                // Carry actual preceding content, not a guessed first-row header.
                if (initialRow.Length == 0) initialRow = text[..Math.Min(text.Length, 1024)];
                context = $"Worksheet: {name}; state: {sheet.State?.Value.ToString() ?? "Visible"}; initial row (not inferred headers): {initialRow}; preceding row: "
                    + text[..Math.Min(text.Length, 1024)];
            }
        }
        result.Issue("SpreadsheetCoverage", "Raw stored values and styles only; visual formatting, merged-cell meaning, drawings and external links are not interpreted.");
        return result.Result();
    }
}
