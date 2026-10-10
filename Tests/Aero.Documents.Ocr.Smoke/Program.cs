using Aero.Documents;
using Aero.Documents.Ocr;
using Aero.Documents.PdfToImage;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

var builder = new PdfDocumentBuilder();
var font = builder.AddStandard14Font(Standard14Font.Helvetica);
var sourcePage = builder.AddPage(PageSize.A4);
sourcePage.AddText("Contract requirements", 30, new PdfPoint(40, 650), font);
var renderer = new PdfToImagePageRenderer();
var sourceImage = await renderer.RenderAsync(builder.Build(), 1, 2480, 3508, 20000000, default);

// One native-text page and one genuinely image-only scan built from the same source.
var mixed = new PdfDocumentBuilder();
var mixedFont = mixed.AddStandard14Font(Standard14Font.Helvetica);
mixed.AddPage(PageSize.A4).AddText("Native contract page", 20, new PdfPoint(40, 650), mixedFont);
var scanned = mixed.AddPage(PageSize.A4);
scanned.AddPng(sourceImage.Png, new PdfRectangle(0, 0, scanned.PageSize.Width, scanned.PageSize.Height));
var pdf = mixed.Build();
var inspection = new PdfPigPageInspector().Inspect(pdf, new(), default);
if (inspection.Pages.Count != 2 || !inspection.Pages[1].HasImages || inspection.Pages[1].Text.Any(char.IsLetterOrDigit))
    throw new InvalidOperationException("Scanned fixture classification failed.");
var processor = new PdfOcrDocumentProcessor(new PdfPigPageInspector(), new ImageOnlyOcrPolicy(),
    renderer, new TesseractProcessEngine(new LocalProcessRunner(), new(),
        new() { Executable = args.Length == 0 ? "tesseract" : args[0] }));
var result = await processor.ExtractAsync(pdf);
if (result.Document.Blocks.Count != 2 || result.OcrPages.Count != 1 || result.OcrPages[0].Page != 2
    || !result.Document.Blocks[1].Text.Contains("Contract requirements", StringComparison.OrdinalIgnoreCase)
    || result.OcrPages[0].Recognition.Words.Count < 2)
    throw new InvalidOperationException("OCR smoke failed: " + string.Join(",", result.Document.Issues.Select(x => x.Code)));
var chunks = new DocumentChunker().Chunk(result.Document);
if (!chunks.Any(x => x.Location.Page == 2 && x.Content.Contains("Contract requirements", StringComparison.OrdinalIgnoreCase)))
    throw new InvalidOperationException("OCR source chunking failed.");
Console.WriteLine($"OCR smoke passed on {System.Runtime.InteropServices.RuntimeInformation.OSDescription}: 2 pages, 1 OCR page, {result.OcrPages[0].Recognition.Words.Count} words, anchored chunks.");
