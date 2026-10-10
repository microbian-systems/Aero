using System.Diagnostics;
using Aero.Documents.Ocr;
using Shouldly;
using TUnit.Core;

namespace Aero.Documents.Tests;

[Category("Integration")]
public sealed class LocalProcessIntegrationTests
{
    private static string Fixture => Path.Combine(AppContext.BaseDirectory, "process-fixture",
        "Aero.Documents.ProcessFixture" + (OperatingSystem.IsWindows() ? ".exe" : ""));

    [Test]
    public async Task ProcessArgumentBoundariesAndPrivateStderrArePreserved()
    {
        string value = "contract name; $not-a-shell | Café";
        var result = await new LocalProcessRunner().RunAsync(new(Fixture,
            ["exact", value], TimeSpan.FromSeconds(10), 1000), default);
        result.ExitCode.ShouldBe(0);
        result.Output.ShouldBe(value);
        result.Output.ShouldNotContain("private diagnostic");
    }

    [Test]
    [Arguments(4, true)]
    [Arguments(5, false)]
    public async Task ProcessOutputIsBounded(int length, bool success)
    {
        var request = new LocalProcessRequest(Fixture, ["exact", new string('x', length)], TimeSpan.FromSeconds(10), 4);
        if (success)
        {
            var result = await new LocalProcessRunner().RunAsync(request, default);
            result.Output.ShouldBe("xxxx");
            result.ExitCode.ShouldBe(0);
        }
        else
        {
            var error = await Should.ThrowAsync<DocumentAdapterException>(() => new LocalProcessRunner().RunAsync(request, default));
            error.Code.ShouldBe("ProcessOutputLimit");
        }
    }

    [Test]
    public async Task ProcessTimeoutIsSanitizedAndChildIsTerminated()
    {
        var workspace = Directory.CreateTempSubdirectory("aero-process-test-");
        try
        {
            var pidPath = Path.Combine(workspace.FullName, "child.pid");
            var error = await Should.ThrowAsync<DocumentAdapterException>(() => new LocalProcessRunner()
                .RunAsync(new(Fixture, ["delay", pidPath], TimeSpan.FromSeconds(2), 1000), default));
            error.Code.ShouldBe("ProcessTimeout");
            File.Exists(pidPath).ShouldBeTrue();
            int pid = int.Parse(File.ReadAllText(pidPath));
            bool running;
            try { using var child = Process.GetProcessById(pid); running = !child.HasExited; }
            catch (ArgumentException) { running = false; }
            running.ShouldBeFalse();
        }
        finally { workspace.Delete(recursive: true); }
    }

    [Test]
    public async Task ProcessCallerCancellationRemainsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(() => new LocalProcessRunner()
            .RunAsync(new(Fixture, ["exact", "text"], TimeSpan.FromSeconds(10), 1000), cancellation.Token));
    }

    [Test]
    public async Task MissingRuntimeIsExplicit()
    {
        string missing = Path.Combine(Path.GetTempPath(), "missing-aero-runtime-" + Path.GetRandomFileName());
        var error = await Should.ThrowAsync<DocumentAdapterException>(() => new LocalProcessRunner()
            .RunAsync(new(missing, [], TimeSpan.FromSeconds(10), 1000), default));
        error.Code.ShouldBe("RuntimeUnavailable");
        error.Message.ShouldNotContain(missing);
    }

    [Test]
    public async Task RunningProcessCancellationTerminatesChild()
    {
        var workspace = Directory.CreateTempSubdirectory("aero-process-test-");
        try
        {
            string pidPath = Path.Combine(workspace.FullName, "child.pid");
            using var cancellation = new CancellationTokenSource();
            var running = new LocalProcessRunner().RunAsync(new(Fixture, ["delay", pidPath],
                TimeSpan.FromSeconds(20), 1000), cancellation.Token);
            using var readiness = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (!File.Exists(pidPath)) await Task.Delay(25, readiness.Token);
            cancellation.Cancel();
            await Should.ThrowAsync<OperationCanceledException>(() => running);
            ChildHasExited(pidPath).ShouldBeTrue();
        }
        finally { workspace.Delete(recursive: true); }
    }

    [Test]
    public async Task InvalidUtf8StopsRunningProcessWithoutWaitingForDeadline()
    {
        var workspace = Directory.CreateTempSubdirectory("aero-process-test-");
        try
        {
            string pidPath = Path.Combine(workspace.FullName, "child.pid");
            var running = new LocalProcessRunner().RunAsync(new(Fixture, ["invalid-utf8", pidPath],
                TimeSpan.FromSeconds(20), 1000), default);
            var error = await Should.ThrowAsync<DocumentAdapterException>(() => running.WaitAsync(TimeSpan.FromSeconds(5)));
            error.Code.ShouldBe("InvalidProcessOutput");
            ChildHasExited(pidPath).ShouldBeTrue();
        }
        finally { workspace.Delete(recursive: true); }
    }

    private static bool ChildHasExited(string pidPath)
    {
        int pid = int.Parse(File.ReadAllText(pidPath));
        try { using var child = Process.GetProcessById(pid); return child.HasExited; }
        catch (ArgumentException) { return true; }
    }
}
