using System.Buffers.Binary;
using Aero.Documents.Ocr;
using Aero.Documents.PdfToImage;
using FluentValidation;
using NSubstitute;
using Shouldly;
using TUnit.Core;

namespace Aero.Documents.Tests;

public sealed class OcrTests
{
    [Test]
    [Arguments("Native text", false, false)]
    [Arguments("Native text", true, false)]
    [Arguments("", true, true)]
    [Arguments("", false, false)]
    [Arguments("   \n", true, true)]
    public void PolicySelectsOnlyImagePagesWithoutUsableText(string text, bool hasImages, bool selected) =>
        new ImageOnlyOcrPolicy().ShouldRecognize(new(1, text, hasImages, 72, 72)).ShouldBe(selected);

    [Test]
    public async Task MixedDocumentUsesOcrOnlyOnScannedPage()
    {
        var inspector = Substitute.For<IPdfPageInspector>();
        inspector.Inspect(Arg.Any<byte[]>(), Arg.Any<ExtractionLimits>(), Arg.Any<CancellationToken>())
            .Returns(new PdfInspection([
                new(1, "Existing native text", false, 72, 72),
                new(2, "", true, 72, 72),
                new(3, "Existing OCR text layer", true, 72, 72)], []));
        var renderer = Substitute.For<IPdfPageRenderer>();
        renderer.RenderAsync(Arg.Any<byte[]>(), 2, 300, 300, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new RenderedPage([], 300, 300));
        var engine = Substitute.For<IOcrEngine>();
        var word = new OcrWord("Scanned", 1, 2, 40, 10, 60, 1, 1, 1);
        engine.RecognizeAsync(Arg.Any<RenderedPage>(), Arg.Any<CancellationToken>())
            .Returns(new OcrResult("test-engine", "Scanned requirements", [word], []));
        var result = await new PdfOcrDocumentProcessor(inspector, new ImageOnlyOcrPolicy(), renderer, engine).ExtractAsync([1]);
        result.Document.Blocks.Select(x => x.Text).ShouldBe(["Existing native text", "Scanned requirements", "Existing OCR text layer"]);
        result.Document.Blocks.Select(x => x.Location.Page).ShouldBe(new int?[] { 1, 2, 3 });
        result.OcrPages.Single().Page.ShouldBe(2);
        result.OcrPages.Single().Recognition.Words.Single().ShouldBe(word);
        result.Document.Issues.Single(x => x.Code == "LowOcrConfidence").Location!.Page.ShouldBe(2);
        result.Document.Issues.Single(x => x.Code == "MixedPageCoverage").Location!.Page.ShouldBe(3);
        result.Document.IsComplete.ShouldBeFalse();
        await renderer.Received(1).RenderAsync(Arg.Any<byte[]>(), 2, 300, 300, Arg.Any<int>(), Arg.Any<CancellationToken>());
        await renderer.DidNotReceive().RenderAsync(Arg.Any<byte[]>(), 1, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await renderer.DidNotReceive().RenderAsync(Arg.Any<byte[]>(), 3, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await engine.Received(1).RecognizeAsync(Arg.Any<RenderedPage>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task BlankPagesAreReportedWithoutOcr()
    {
        var inspector = Substitute.For<IPdfPageInspector>();
        inspector.Inspect(Arg.Any<byte[]>(), Arg.Any<ExtractionLimits>(), Arg.Any<CancellationToken>())
            .Returns(new PdfInspection([new(1, "", false, 72, 72)], []));
        var renderer = Substitute.For<IPdfPageRenderer>();
        var engine = Substitute.For<IOcrEngine>();
        var result = await new PdfOcrDocumentProcessor(inspector, new ImageOnlyOcrPolicy(), renderer, engine).ExtractAsync([1]);
        result.Document.Blocks.ShouldBeEmpty();
        result.Document.Issues.Single(x => x.Code == "NoPageText").Location!.Page.ShouldBe(1);
        result.OcrPages.ShouldBeEmpty();
        await engine.DidNotReceive().RecognizeAsync(Arg.Any<RenderedPage>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task OcrBudgetsKeepCoverageExplicit()
    {
        var inspector = Substitute.For<IPdfPageInspector>();
        inspector.Inspect(Arg.Any<byte[]>(), Arg.Any<ExtractionLimits>(), Arg.Any<CancellationToken>())
            .Returns(new PdfInspection([new(1, "", true, 72, 72), new(2, "", true, 72, 72)], []));
        var renderer = Substitute.For<IPdfPageRenderer>();
        renderer.RenderAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new RenderedPage([], 300, 300));
        var engine = Substitute.For<IOcrEngine>();
        engine.RecognizeAsync(Arg.Any<RenderedPage>(), Arg.Any<CancellationToken>()).Returns(new OcrResult("test", "First", [], []));
        var result = await new PdfOcrDocumentProcessor(inspector, new ImageOnlyOcrPolicy(), renderer, engine,
            new() { MaxOcrPages = 1 }).ExtractAsync([1]);
        result.Document.Blocks.Single().Text.ShouldBe("First");
        result.Document.Issues.Single(x => x.Code == "OcrPageLimit").Location!.Page.ShouldBe(2);
        await engine.Received(1).RecognizeAsync(Arg.Any<RenderedPage>(), Arg.Any<CancellationToken>());
        var pixelLimited = await new PdfOcrDocumentProcessor(inspector, new ImageOnlyOcrPolicy(), renderer, engine,
            new() { MaxPixels = 89999 }).ExtractAsync([1]);
        pixelLimited.Document.Blocks.ShouldBeEmpty();
        pixelLimited.Document.Issues.Count(x => x.Code == "RasterLimit").ShouldBe(2);
        var textLimited = await new PdfOcrDocumentProcessor(inspector, new ImageOnlyOcrPolicy(), renderer, engine)
            .ExtractAsync([1], new() { MaxCharacters = 4 });
        textLimited.Document.Blocks.ShouldBeEmpty();
        textLimited.Document.Issues.Select(x => x.Code).ShouldContain("TextLimit");
    }

    [Test]
    public async Task AdapterFailuresAreAnchoredAndCancellationPropagates()
    {
        var inspector = Substitute.For<IPdfPageInspector>();
        inspector.Inspect(Arg.Any<byte[]>(), Arg.Any<ExtractionLimits>(), Arg.Any<CancellationToken>())
            .Returns(new PdfInspection([new(7, "", true, 72, 72)], []));
        var renderer = Substitute.For<IPdfPageRenderer>();
        renderer.RenderAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<RenderedPage>(new DocumentAdapterException("RenderFailed", "Safe failure.")));
        var engine = Substitute.For<IOcrEngine>();
        var processor = new PdfOcrDocumentProcessor(inspector, new ImageOnlyOcrPolicy(), renderer, engine);
        var failed = await processor.ExtractAsync([1]);
        failed.Document.Issues.Single(x => x.Code == "RenderFailed").Location!.Page.ShouldBe(7);
        await engine.DidNotReceive().RecognizeAsync(Arg.Any<RenderedPage>(), Arg.Any<CancellationToken>());
        renderer.RenderAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<RenderedPage>(new OperationCanceledException()));
        await Should.ThrowAsync<OperationCanceledException>(() => processor.ExtractAsync([1]));
    }

    [Test]
    public void TsvPreservesLinesBoxesAndConfidence()
    {
        var result = new TesseractTsvParser().Parse(Tsv(
            "5\t1\t1\t1\t1\t1\t10\t20\t50\t10\t95.5\tContract",
            "5\t1\t1\t1\t1\t2\t70\t20\t50\t10\t80\trequirements",
            "5\t1\t1\t1\t2\t1\t10\t40\t40\t10\t60\t$100"), 300, 300);
        result.Text.ShouldBe("Contract requirements\n$100");
        result.Words[0].ShouldBe(new OcrWord("Contract", 10, 20, 50, 10, 95.5, 1, 1, 1));
        result.Words[2].Line.ShouldBe(2);
        result.Issues.ShouldBeEmpty();
    }

    [Test]
    [Arguments("bad header", "InvalidOcrOutput")]
    [Arguments("empty", "NoOcrText")]
    [Arguments("bad confidence", "InvalidOcrOutput")]
    [Arguments("bad geometry", "InvalidOcrOutput")]
    public void InvalidAndEmptyTsvAreExplicit(string kind, string code)
    {
        var tsv = kind switch
        {
            "bad header" => "wrong header",
            "bad confidence" => Tsv("5\t1\t1\t1\t1\t1\t10\t20\t50\t10\tNaN\tContract"),
            "bad geometry" => Tsv("5\t1\t1\t1\t1\t1\t299\t20\t50\t10\t90\tContract"),
            _ => Tsv()
        };
        var result = new TesseractTsvParser().Parse(tsv, 300, 300);
        result.Words.ShouldBeEmpty();
        result.Text.ShouldBeEmpty();
        result.Issues.Single().Code.ShouldBe(code);
    }

    [Test]
    public void TsvLimitsPreserveOnlyCompleteWords()
    {
        var input = Tsv("5\t1\t1\t1\t1\t1\t10\t20\t50\t10\t90\tContract",
            "5\t1\t1\t1\t1\t2\t70\t20\t50\t10\t90\trequirements");
        var result = new TesseractTsvParser().Parse(input, 300, 300, maxWords: 1);
        result.Text.ShouldBe("Contract");
        result.Words.Count.ShouldBe(1);
        result.Issues.Single().Code.ShouldBe("OcrOutputLimit");
        var charLimited = new TesseractTsvParser().Parse(input, 300, 300, maxTextCharacters: 8);
        charLimited.Text.ShouldBe("Contract");
        charLimited.Issues.Single().Code.ShouldBe("OcrOutputLimit");
    }

    [Test]
    public async Task TesseractUsesArgumentListAndCleansItsOwnedWorkspace()
    {
        var runner = Substitute.For<ILocalProcessRunner>();
        string? imagePath = null;
        runner.RunAsync(Arg.Any<LocalProcessRequest>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var request = call.Arg<LocalProcessRequest>();
            request.Executable.ShouldBe("tesseract");
            request.Arguments.Skip(1).ShouldBe(["stdout", "-l", "eng", "--oem", "1", "tsv"]);
            request.Environment!["OMP_THREAD_LIMIT"].ShouldBe("1");
            imagePath = request.Arguments[0];
            File.Exists(imagePath).ShouldBeTrue();
            return Task.FromResult(new LocalProcessResult(0, Tsv("5\t1\t1\t1\t1\t1\t10\t20\t50\t10\t90\tContract")));
        });
        var engine = new TesseractProcessEngine(runner, new());
        var result = await engine.RecognizeAsync(PngHeader(300, 300), default);
        result.Text.ShouldBe("Contract");
        result.Issues.ShouldBeEmpty();
        imagePath.ShouldNotBeNull();
        File.Exists(imagePath).ShouldBeFalse();
        Directory.Exists(Path.GetDirectoryName(imagePath)).ShouldBeFalse();
    }

    [Test]
    public async Task TesseractOperationalFailuresAreSafeAndCancellationPropagates()
    {
        var runner = Substitute.For<ILocalProcessRunner>();
        runner.RunAsync(Arg.Any<LocalProcessRequest>(), Arg.Any<CancellationToken>())
            .Returns(new LocalProcessResult(1, "private runtime diagnostics"));
        var engine = new TesseractProcessEngine(runner, new());
        var result = await engine.RecognizeAsync(PngHeader(300, 300), default);
        result.Text.ShouldBeEmpty();
        result.Issues.Single().Code.ShouldBe("OcrFailed");
        result.Issues.Single().Message.ShouldNotContain("private");
        runner.RunAsync(Arg.Any<LocalProcessRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<LocalProcessResult>(new DocumentAdapterException("ProcessTimeout", "Safe timeout.")));
        (await engine.RecognizeAsync(PngHeader(300, 300), default)).Issues.Single().Code.ShouldBe("ProcessTimeout");
        runner.RunAsync(Arg.Any<LocalProcessRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<LocalProcessResult>(new OperationCanceledException()));
        await Should.ThrowAsync<OperationCanceledException>(() => engine.RecognizeAsync(PngHeader(300, 300), default));
    }

    [Test]
    public async Task InvalidPngDoesNotLaunchOcr()
    {
        var runner = Substitute.For<ILocalProcessRunner>();
        var engine = new TesseractProcessEngine(runner, new());
        var result = await engine.RecognizeAsync(new([], 300, 300), default);
        result.Issues.Single().Code.ShouldBe("InvalidImage");
        await runner.DidNotReceive().RunAsync(Arg.Any<LocalProcessRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task PdfInspectorAndRendererUseOneBasedPagesAndRealNativeOutput()
    {
        var pdf = DocumentProcessorTests.Pdf("Contract requirements", "Second page");
        var inspector = new PdfPigPageInspector().Inspect(pdf, new(), default);
        inspector.Pages.Select(x => x.Number).ShouldBe([1, 2]);
        inspector.Pages[0].Text.ShouldContain("Contract requirements");
        inspector.Pages.ShouldAllBe(x => !x.HasImages);
        var image = await new PdfToImagePageRenderer().RenderAsync(pdf, 2, 300, 420, 1000000, default);
        image.Width.ShouldBe(300);
        image.Height.ShouldBe(420);
        image.Png.Take(8).ShouldBe(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var capped = new PdfPigPageInspector().Inspect(pdf, new() { MaxUnits = 1 }, default);
        capped.Pages.Count.ShouldBe(1);
        capped.Issues.Single().Code.ShouldBe("UnitLimit");
        var error = await Should.ThrowAsync<DocumentAdapterException>(() => new PdfToImagePageRenderer()
            .RenderAsync(pdf, 1, 300, 420, 1, default));
        error.Code.ShouldBe("RasterLimit");
    }

    [Test]
    public async Task InvalidConfigurationAndInputAreRejectedBeforeProvidersRun()
    {
        var inspector = Substitute.For<IPdfPageInspector>();
        var renderer = Substitute.For<IPdfPageRenderer>();
        var engine = Substitute.For<IOcrEngine>();
        Should.Throw<ValidationException>(() => new PdfOcrDocumentProcessor(inspector, new ImageOnlyOcrPolicy(), renderer, engine, new() { Dpi = 0 }));
        var processor = new PdfOcrDocumentProcessor(inspector, new ImageOnlyOcrPolicy(), renderer, engine);
        var limited = await processor.ExtractAsync([1, 2], new() { MaxInputBytes = 1 });
        limited.Document.Issues.Single().Code.ShouldBe("InputLimit");
        inspector.DidNotReceive().Inspect(Arg.Any<byte[]>(), Arg.Any<ExtractionLimits>(), Arg.Any<CancellationToken>());
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(() => processor.ExtractAsync([], cancellationToken: cancelled.Token));
    }

    internal static string Tsv(params string[] rows) =>
        "level\tpage_num\tblock_num\tpar_num\tline_num\tword_num\tleft\ttop\twidth\theight\tconf\ttext\n" + string.Join("\n", rows);
    internal static RenderedPage PngHeader(int width, int height)
    {
        byte[] png = new byte[33];
        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }.CopyTo(png, 0);
        BinaryPrimitives.WriteUInt32BigEndian(png.AsSpan(8, 4), 13);
        "IHDR"u8.CopyTo(png.AsSpan(12));
        BinaryPrimitives.WriteUInt32BigEndian(png.AsSpan(16, 4), (uint)width);
        BinaryPrimitives.WriteUInt32BigEndian(png.AsSpan(20, 4), (uint)height);
        return new(png, width, height);
    }
}
