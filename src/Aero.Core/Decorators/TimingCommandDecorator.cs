using System.Diagnostics;
using Aero.Core.Commands;

namespace Aero.Core.Decorators;

/// <summary>
/// Represents a class for TimingCommandDecorator.
/// </summary>
public class TimingCommandDecorator(IAsyncCommand decorated, ILogger log) : IAsyncCommand
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task ExecuteAsync()
    {
        log.LogInformation($"entered Timing decorator");
        var sw = new Stopwatch();
        sw.Start();
        await decorated.ExecuteAsync();
        sw.Stop();
        log.LogInformation($"{decorated.GetType()} took {sw.ElapsedMilliseconds} ms");
    }
}

/// <summary>
/// Represents a class for TimingCommandDecorator.
/// </summary>
public class TimingCommandDecorator<TCommand>(IAsyncCommand<TCommand> decorated, ILogger log) : IAsyncCommand<TCommand>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task ExecuteAsync(TCommand param)
    {
        var sw = new Stopwatch();
        sw.Start();
        await decorated.ExecuteAsync(param);
        sw.Stop();
        log.LogInformation($"{decorated.GetType()} took {sw.ElapsedMilliseconds} ms");
    }
}

/// <summary>
/// Represents a class for TimingCommandDecorator.
/// </summary>
public class TimingCommandDecorator<TCommand, TReturn>(IAsyncCommand<TCommand, TReturn> decorated, ILogger log)
    : IAsyncCommand<TCommand, TReturn>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task<TReturn> ExecuteAsync(TCommand param)
    {
        var sw = new Stopwatch();
        sw.Start();
        var result = await decorated.ExecuteAsync(param);
        sw.Stop();
        log.LogInformation($"{decorated.GetType()} took {sw.ElapsedMilliseconds} ms");
        return result;
    }
}