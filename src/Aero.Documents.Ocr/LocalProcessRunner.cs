using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Aero.Documents.Ocr;

/// <summary>Trusted executable and arguments; never assembled into a shell command.</summary>
public sealed record LocalProcessRequest(string Executable, IReadOnlyList<string> Arguments,
    TimeSpan Timeout, int MaxOutputCharacters, IReadOnlyDictionary<string, string>? Environment = null);
/// <summary>Sanitized result; stderr is drained but never retained or logged.</summary>
public sealed record LocalProcessResult(int ExitCode, string Output);
/// <summary>Process boundary used by the CLI adapter.</summary>
public interface ILocalProcessRunner
{
    /// <summary>Propagates caller cancellation; operational failures use DocumentAdapterException.</summary>
    Task<LocalProcessResult> RunAsync(LocalProcessRequest request, CancellationToken cancellationToken);
}
/// <summary>No shell; bounded stdout, discarded stderr, timeout and best-effort process-tree termination.</summary>
public sealed class LocalProcessRunner : ILocalProcessRunner
{
    /// <inheritdoc />
    public async Task<LocalProcessResult> RunAsync(LocalProcessRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Executable);
        ArgumentOutOfRangeException.ThrowIfLessThan(request.MaxOutputCharacters, 1);
        if (request.Timeout <= TimeSpan.Zero || request.Timeout > TimeSpan.FromMinutes(10))
            throw new ArgumentOutOfRangeException(nameof(request));
        cancellationToken.ThrowIfCancellationRequested();
        var start = new ProcessStartInfo(request.Executable)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = new UTF8Encoding(false, true)
        };
        foreach (var argument in request.Arguments) start.ArgumentList.Add(argument);
        foreach (var pair in request.Environment ?? new Dictionary<string, string>()) start.Environment[pair.Key] = pair.Value;
        using var process = new Process { StartInfo = start };
        try { process.Start(); }
        catch (Win32Exception) { throw new DocumentAdapterException("RuntimeUnavailable", "Local document runtime could not be started."); }
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(request.Timeout);
        Task<string> stdout = ReadOutput(process.StandardOutput, request.MaxOutputCharacters, deadline);
        Task stderr = DiscardOutput(process.StandardError, deadline.Token);
        try
        {
            await Task.WhenAll(stdout, stderr, process.WaitForExitAsync(deadline.Token));
            return new(process.ExitCode, await stdout);
        }
        catch (OperationCanceledException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new DocumentAdapterException("ProcessTimeout", "Local document process timed out.");
        }
        catch (DecoderFallbackException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new DocumentAdapterException("InvalidProcessOutput", "Runtime output is not valid UTF-8.");
        }
        finally
        {
            deadline.Cancel();
            if (!process.HasExited)
            {
                try { process.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) { }
                catch (Win32Exception) { }
                using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                try { await process.WaitForExitAsync(cleanup.Token); }
                catch (OperationCanceledException) { }
            }
        }
    }
    private static async Task<string> ReadOutput(StreamReader reader, int limit, CancellationTokenSource deadline)
    {
        var output = new StringBuilder();
        char[] scratch = new char[4096];
        int read;
        try
        {
            while ((read = await reader.ReadAsync(scratch, deadline.Token)) > 0)
            {
                if (read > limit - output.Length)
                    throw new DocumentAdapterException("ProcessOutputLimit", "Local document process exceeded its output budget.");
                output.Append(scratch, 0, read);
            }
        }
        catch { deadline.Cancel(); throw; }
        return output.ToString();
    }
    private static async Task DiscardOutput(StreamReader reader, CancellationToken cancellationToken)
    {
        char[] scratch = new char[4096];
        while (await reader.ReadAsync(scratch, cancellationToken) > 0) { }
    }
}
