using System.Text;
using System.IO.Compression;
using Aero.Documents;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using FluentValidation;
using Shouldly;
using TUnit.Core;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using W = DocumentFormat.OpenXml.Wordprocessing;
using S = DocumentFormat.OpenXml.Spreadsheet;

namespace Aero.Documents.Tests;

public sealed class DocumentProcessorTests
{
    [Test]
    public async Task TextRoutingIsCaseInsensitiveAndStreamStaysOpen()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Scope of work\nSupply meals.\n\nSubmit proposal."));
        var result = await DocumentProcessorRegistry.CreateDefault().ExtractAsync("NOTICE.TXT", stream);
        result.Processor.ShouldBe("txt:1");
        result.IsComplete.ShouldBeTrue();
        result.Blocks.Select(x => x.Text).ShouldBe(["Scope of work\nSupply meals.", "Submit proposal."]);
        result.Blocks.Select(x => x.Location.Part).ShouldBe(["lines:1-2", "lines:4-4"]);
        stream.CanRead.ShouldBeTrue();
    }

    [Test]
    [Arguments("utf8")]
    [Arguments("utf16le")]
    [Arguments("utf16be")]
    [Arguments("utf32")]
    public void TextEncodingsAreDecoded(string name)
    {
        Encoding encoding = name switch
        {
            "utf16le" => new UnicodeEncoding(false, true, true),
            "utf16be" => new UnicodeEncoding(true, true, true),
            "utf32" => new UTF32Encoding(false, true, true),
            _ => new UTF8Encoding(true, true)
        };
        byte[] bytes = [.. encoding.GetPreamble(), .. encoding.GetBytes("Café — price €100")];
        var result = new PlainTextDocumentProcessor().Process(bytes, new());
        result.Blocks.Single().Text.ShouldBe("Café — price €100");
        result.Issues.ShouldBeEmpty();
    }

    [Test]
    public void InvalidUtf8IsNotReplacedSilently()
    {
        var result = new PlainTextDocumentProcessor().Process([0xc3, 0x28], new());
        result.Blocks.ShouldBeEmpty();
        result.Issues.Single().Code.ShouldBe("InvalidDocument");
        new PlainTextDocumentProcessor().Process([65, 0, 66], new()).Issues.Single().Code.ShouldBe("InvalidText");
    }

    [Test]
    [Arguments(3, true)]
    [Arguments(4, false)]
    public async Task InputLimitIsExact(int bytes, bool success)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(new string('a', bytes)));
        var result = await DocumentProcessorRegistry.CreateDefault().ExtractAsync("a.txt", stream, new() { MaxInputBytes = 3 });
        result.IsComplete.ShouldBe(success);
        if (success) result.Blocks.Single().Text.ShouldBe("aaa");
        else { result.Blocks.ShouldBeEmpty(); result.Issues.Single().Code.ShouldBe("InputLimit"); }
        stream.CanRead.ShouldBeTrue();
    }

    [Test]
    public async Task UnsupportedAndMismatchedFormatsAreExplicit()
    {
        using var stream = new MemoryStream([0x50, 0x4b]);
        var result = await DocumentProcessorRegistry.CreateDefault().ExtractAsync("old.doc", stream);
        result.Issues.Single().Code.ShouldBe("UnsupportedFormat");
        stream.Position.ShouldBe(0);
        new PdfDocumentProcessor().Process("not PDF"u8.ToArray(), new()).Issues.Single().Code.ShouldBe("FormatMismatch");
        new WordDocumentProcessor().Process("%PDF-fake"u8.ToArray(), new()).Blocks.ShouldBeEmpty();
    }

    [Test]
    public async Task RegistrySelectsOneProcessorPerSupportedExtension()
    {
        var registry = DocumentProcessorRegistry.CreateDefault();
        foreach (var (name, bytes, id) in new[]
        {
            ("a.txt", "hello"u8.ToArray(), "txt:1"),
            ("a.pdf", Pdf("Page one"), "pdfpig:1"),
            ("a.docx", Word(), "openxml-docx:1"),
            ("a.xlsx", Spreadsheet(), "openxml-xlsx:1")
        })
        {
            using var stream = new MemoryStream(bytes);
            var result = await registry.ExtractAsync(name, stream);
            result.Processor.ShouldBe(id);
            result.Blocks.ShouldNotBeEmpty();
        }
    }

    [Test]
    public void PdfTextHasPageAnchor()
    {
        var result = new PdfDocumentProcessor().Process(Pdf("Contract requirements", "Submit by Friday"), new());
        result.Blocks.Select(x => x.Location.Page).ShouldBe(new int?[] { 1, 2 });
        result.Blocks[0].Text.ShouldContain("Contract requirements");
        result.Blocks[1].Text.ShouldContain("Submit by Friday");
        result.Issues.Single().Code.ShouldBe("LayoutCoverage");
    }

    [Test]
    public void PdfPageLimitIsReported()
    {
        var result = new PdfDocumentProcessor().Process(Pdf("First", "Second"), new() { MaxUnits = 1 });
        result.Blocks.Count.ShouldBe(1);
        result.Blocks[0].Text.ShouldContain("First");
        result.Issues.Select(x => x.Code).ShouldContain("UnitLimit");
        result.IsComplete.ShouldBeFalse();
    }

    [Test]
    public void PdfEmptyPageIsNotComplete()
    {
        var result = new PdfDocumentProcessor().Process(Pdf("", "Readable page"), new());
        result.Blocks.Count.ShouldBe(1);
        result.Blocks.Single().Location.Page.ShouldBe(2);
        result.Issues.Single(x => x.Code == "NoPageText").Location!.Page.ShouldBe(1);
        result.IsComplete.ShouldBeFalse();
    }

    [Test]
    public void WordParagraphsAndTableRowsHaveAnchors()
    {
        var result = new WordDocumentProcessor().Process(Word(), new());
        result.Blocks[0].Text.ShouldBe("Scope");
        result.Blocks[1].Text.ShouldBe("Supply equipment");
        result.Blocks[1].Context.ShouldBe("Scope");
        result.Blocks[2].Text.ShouldBe("Column 1: Item | Column 2: Price");
        result.Blocks[3].Text.ShouldBe("Column 1: Meals | Column 2: $100");
        result.Blocks[3].Context.ShouldContain("Column 1: Item | Column 2: Price");
        result.Blocks[3].Location.Part.ShouldBe("body:table:3:row:2");
        result.Issues.Single().Code.ShouldBe("BodyCoverage");
    }

    [Test]
    public void SpreadsheetKeepsRowsCoordinatesFormulasAndContext()
    {
        var result = new SpreadsheetDocumentProcessor().Process(Spreadsheet(), new());
        result.Blocks.Count.ShouldBe(2);
        var row = result.Blocks[1];
        row.Location.ShouldBe(new DocumentLocation("worksheet", Sheet: "Pricing", Row: 2, CellRange: "A2:E2"));
        row.Context.ShouldContain("A1: value=Item");
        row.Context.ShouldContain("B1: value=Unit price");
        row.Text.ShouldContain("A2: value=Meals");
        row.Text.ShouldContain("B2: value=12.50");
        row.Text.ShouldContain("formatCode=$0.00");
        row.Text.ShouldContain("C2: value=25");
        row.Text.ShouldContain("formula=B2*2");
        row.Text.ShouldContain("D2: value=true");
        row.Text.ShouldContain("E2: value=#REF!");
        result.Issues.Select(x => x.Code).ShouldContain("CachedFormulaValues");
        result.Issues.Select(x => x.Code).ShouldContain("CellError");
        result.IsComplete.ShouldBeFalse();
    }

    [Test]
    public void SpreadsheetCellLimitIsReported()
    {
        var result = new SpreadsheetDocumentProcessor().Process(Spreadsheet(), new() { MaxCells = 3 });
        result.Blocks.Count.ShouldBe(1);
        result.Blocks[0].Location.Row.ShouldBe((uint)1);
        result.Issues.Single().Code.ShouldBe("CellLimit");
    }

    [Test]
    public void OfficeExpansionLimitIsEnforced()
    {
        var result = new WordDocumentProcessor().Process(Word(), new() { MaxExpandedBytes = 1 });
        result.Blocks.ShouldBeEmpty();
        result.Issues.Single().Code.ShouldBe("ExpandedLimit");
    }

    [Test]
    public void MacroPayloadsAndEntryLimitsAreRejected()
    {
        using var stream = new MemoryStream();
        stream.Write(Word());
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Update, true))
        {
            using var macro = zip.CreateEntry("word/vbaProject.bin").Open();
            macro.WriteByte(1);
        }
        var result = new WordDocumentProcessor().Process(stream.ToArray(), new());
        result.Blocks.ShouldBeEmpty();
        result.Issues.Single().Code.ShouldBe("UnsupportedMacros");
        var entries = new WordDocumentProcessor().Process(Word(), new() { MaxArchiveEntries = 1 });
        entries.Blocks.ShouldBeEmpty();
        entries.Issues.Single().Code.ShouldBe("ArchiveLimit");
    }

    [Test]
    public void SpreadsheetInitialRowSurvivesLaterRowsAndMissingCachesAreExplicit()
    {
        using var stream = new MemoryStream();
        stream.Write(Spreadsheet());
        using (var document = SpreadsheetDocument.Open(stream, true))
        {
            var sheet = document.WorkbookPart!.WorksheetParts.Single();
            sheet.Worksheet!.GetFirstChild<S.SheetData>()!.Append(new S.Row(
                new S.Cell { CellReference = "A3", DataType = S.CellValues.InlineString, InlineString = new(new S.Text("Delivery")) },
                new S.Cell { CellReference = "B3", CellFormula = new("SUM(B2:B2)") }) { RowIndex = 3 });
        }
        var result = new SpreadsheetDocumentProcessor().Process(stream.ToArray(), new());
        result.Blocks[2].Context.ShouldContain("initial row (not inferred headers): A1: value=Item");
        result.Blocks[2].Context.ShouldContain("B1: value=Unit price");
        result.Blocks[2].Location.Row.ShouldBe((uint)3);
        result.Issues.Single(x => x.Code == "MissingFormulaResult").Location!.CellRange.ShouldBe("B3");
        var chunks = new DocumentChunker().Chunk(result);
        chunks.Where(x => x.BlockOrdinal == 2).ShouldNotBeEmpty();
        chunks.Where(x => x.BlockOrdinal == 2).ShouldAllBe(x => x.Context.Contains("initial row"));
    }

    [Test]
    public async Task NonSeekableStreamsAreReadWithoutOwnershipTransfer()
    {
        using var stream = new NonSeekableInput("Scope"u8.ToArray());
        var result = await DocumentProcessorRegistry.CreateDefault().ExtractAsync("scope.txt", stream);
        result.Blocks.Single().Text.ShouldBe("Scope");
        stream.CanRead.ShouldBeTrue();
        stream.CanSeek.ShouldBeFalse();
    }

    [Test]
    public async Task CancellationPropagates()
    {
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Should.Throw<OperationCanceledException>(() => new PlainTextDocumentProcessor().Process("text"u8.ToArray(), new(), cancelled.Token));
        using var stream = new MemoryStream("text"u8.ToArray());
        await Should.ThrowAsync<OperationCanceledException>(() => DocumentProcessorRegistry.CreateDefault().ExtractAsync("a.txt", stream, cancellationToken: cancelled.Token));
        Should.Throw<OperationCanceledException>(() => new DocumentChunker().Chunk(new("test", [], []), cancellationToken: cancelled.Token));
    }

    [Test]
    public void InvalidLimitsAreRejected()
    {
        Should.Throw<ValidationException>(() => new PlainTextDocumentProcessor().Process([], new() { MaxUnits = 0 }));
        Should.Throw<ValidationException>(() => new DocumentChunker().Chunk(new("test", [], []), new() { MaxCharacters = 128, MaxContextCharacters = 100 }));
    }

    [Test]
    public void ChunksKeepAllTextAndAnchors()
    {
        var text = string.Concat(Enumerable.Repeat("Deliver 🚀 equipment with warranty. ", 40));
        var location = new DocumentLocation("worksheet", Sheet: "Pricing", Row: 2, CellRange: "A2:B2");
        var document = new ExtractionResult("test", [new(text, location, "Item | Quantity | Unit price")], []);
        var chunks = new DocumentChunker().Chunk(document, new() { MaxCharacters = 128, MaxContextCharacters = 40 });
        chunks.Count.ShouldBeGreaterThan(1);
        string.Concat(chunks.Select(x => x.Content)).ShouldBe(text);
        chunks.Select(x => x.Ordinal).ShouldBe(Enumerable.Range(0, chunks.Count));
        int offset = 0;
        foreach (var chunk in chunks)
        {
            chunk.Location.ShouldBe(location);
            chunk.CharacterOffset.ShouldBe(offset);
            chunk.Context.ShouldBe("Item | Quantity | Unit price");
            (chunk.Content.Length + chunk.Context.Length).ShouldBeLessThanOrEqualTo(128);
            char.IsHighSurrogate(chunk.Content[^1]).ShouldBeFalse();
            offset += chunk.Content.Length;
        }
    }

    [Test]
    public void TextAndUnitLimitsAreCoverageGaps()
    {
        var processor = new PlainTextDocumentProcessor();
        var limited = processor.Process("First\n\nSecond"u8.ToArray(), new() { MaxUnits = 2 });
        limited.Blocks.Single().Text.ShouldBe("First");
        limited.Issues.Single().Code.ShouldBe("UnitLimit");
        var tooLong = processor.Process("Long text"u8.ToArray(), new() { MaxCharacters = 5 });
        tooLong.Blocks.ShouldBeEmpty();
        tooLong.Issues.Single().Code.ShouldBe("TextLimit");
    }

    [Test]
    [Arguments("pdf")]
    [Arguments("docx")]
    [Arguments("xlsx")]
    [Arguments("txt")]
    public void ProcessorsShareLimitValidationAndCancellationContract(string extension)
    {
        IDocumentProcessor processor = extension switch
        {
            "pdf" => new PdfDocumentProcessor(),
            "docx" => new WordDocumentProcessor(),
            "xlsx" => new SpreadsheetDocumentProcessor(),
            _ => new PlainTextDocumentProcessor()
        };
        var tooLarge = processor.Process([1, 2], new() { MaxInputBytes = 1 });
        tooLarge.Blocks.ShouldBeEmpty();
        tooLarge.Issues.Single().Code.ShouldBe("InputLimit");
        Should.Throw<ValidationException>(() => processor.Process([], new() { MaxCells = 0 }));
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Should.Throw<OperationCanceledException>(() => processor.Process([], new(), cancelled.Token));
    }

    [Test]
    public void ChunksDoNotSplitSurrogatePairsAtExactBoundary()
    {
        string text = new string('x', 127) + "🚀" + new string('y', 128);
        var result = new DocumentChunker().Chunk(new("test", [new(text, new("body"))], []),
            new() { MaxCharacters = 128, MaxContextCharacters = 0 });
        result[0].Content.ShouldBe(new string('x', 127));
        result[1].Content.ShouldStartWith("🚀");
        result[1].CharacterOffset.ShouldBe(127);
        result.ShouldAllBe(x => x.Content.Length <= 128 && !char.IsHighSurrogate(x.Content[x.Content.Length - 1]));
        string.Concat(result.Select(x => x.Content)).ShouldBe(text);
    }

    [Test]
    public void InvalidSpreadsheetReferencesAreAnchoredCoverageGaps()
    {
        using var stream = new MemoryStream();
        stream.Write(Spreadsheet());
        using (var document = SpreadsheetDocument.Open(stream, true))
        {
            var cells = document.WorkbookPart!.WorksheetParts.Single().Worksheet!.Descendants<S.Cell>();
            cells.Single(x => x.CellReference == "A2").CellValue = new("999");
            cells.Single(x => x.CellReference == "D2").CellValue = new("2");
        }
        var result = new SpreadsheetDocumentProcessor().Process(stream.ToArray(), new());
        result.Blocks[1].Text.ShouldContain("A2: value=[invalid shared-string index]");
        result.Issues.Single(x => x.Code == "InvalidSharedString").Location!.CellRange.ShouldBe("A2");
        result.Issues.Single(x => x.Code == "InvalidBoolean").Location!.CellRange.ShouldBe("D2");
        result.IsComplete.ShouldBeFalse();
    }

    internal static byte[] Pdf(params string[] pages)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        foreach (var text in pages)
        {
            var page = builder.AddPage(PageSize.A4);
            if (text.Length != 0) page.AddText(text, 12, new PdfPoint(25, 520), font);
        }
        return builder.Build();
    }

    internal static byte[] Word()
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var part = document.AddMainDocumentPart();
            W.Paragraph P(string text) => new(new W.Run(new W.Text(text)));
            W.TableRow R(params string[] cells) => new(cells.Select(x => new W.TableCell(P(x))));
            var heading = P("Scope");
            heading.ParagraphProperties = new(new W.ParagraphStyleId { Val = "Heading1" });
            part.Document = new(new W.Body(heading, P("Supply equipment"), new W.Table(R("Item", "Price"), R("Meals", "$100"))));
        }
        return stream.ToArray();
    }

    internal static byte[] Spreadsheet()
    {
        using var stream = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var part = document.AddWorkbookPart();
            part.Workbook = new(new S.Sheets());
            var shared = part.AddNewPart<SharedStringTablePart>();
            shared.SharedStringTable = new(new S.SharedStringItem(new S.Text("Item")), new S.SharedStringItem(new S.Text("Meals")));
            var styles = part.AddNewPart<WorkbookStylesPart>();
            styles.Stylesheet = new(new S.NumberingFormats(new S.NumberingFormat { NumberFormatId = 164, FormatCode = "$0.00" }),
                new S.CellFormats(new S.CellFormat { NumberFormatId = 0 }, new S.CellFormat { NumberFormatId = 164 }));
            var sheet = part.AddNewPart<WorksheetPart>();
            S.Cell Inline(string address, string text) => new() { CellReference = address, DataType = S.CellValues.InlineString, InlineString = new(new S.Text(text)) };
            var row1 = new S.Row { RowIndex = 1 };
            row1.Append(new S.Cell { CellReference = "A1", DataType = S.CellValues.SharedString, CellValue = new("0") }, Inline("B1", "Unit price"));
            var row2 = new S.Row { RowIndex = 2 };
            row2.Append(new S.Cell { CellReference = "A2", DataType = S.CellValues.SharedString, CellValue = new("1") },
                new S.Cell { CellReference = "B2", CellValue = new("12.50"), StyleIndex = 1 },
                new S.Cell { CellReference = "C2", CellFormula = new("B2*2"), CellValue = new("25") },
                new S.Cell { CellReference = "D2", DataType = S.CellValues.Boolean, CellValue = new("1") },
                new S.Cell { CellReference = "E2", DataType = S.CellValues.Error, CellValue = new("#REF!") });
            sheet.Worksheet = new(new S.SheetData(row1, row2));
            part.Workbook.Sheets!.Append(new S.Sheet { Id = part.GetIdOfPart(sheet), SheetId = 1, Name = "Pricing" });
        }
        return stream.ToArray();
    }
}

internal sealed class NonSeekableInput(byte[] content) : Stream
{
    private readonly MemoryStream inner = new(content, false);
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => inner.ReadAsync(buffer, cancellationToken);
    public override void Flush() => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
}
