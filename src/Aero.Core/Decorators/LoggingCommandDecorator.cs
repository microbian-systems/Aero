using Aero.Core.Commands;

namespace Aero.Core.Decorators;

/// <summary>
/// Represents a class for LoggingCommandDecorator.
/// </summary>
public class LoggingCommandDecorator(IAsyncCommand decorated, ILogger log) : IAsyncCommand
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task ExecuteAsync()
    {
        var type = decorated.GetType();
        log.LogInformation($"starting Execute on {type}");
        await decorated.ExecuteAsync();
        log.LogInformation($"finished Execute() on {type}");
    }
}
    
/// <summary>
/// Represents a class for LoggingCommandDecorator.
/// </summary>
public class LoggingCommandDecorator<TCommand>(IAsyncCommand<TCommand> decorated, ILogger log) : IAsyncCommand<TCommand>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task ExecuteAsync(TCommand param)
    {
        var type = decorated.GetType();
        log.LogInformation($"starting Execute on {type}");
        await decorated.ExecuteAsync(param);
        log.LogInformation($"finished Execute() on {type}");
    }
}
    
/// <summary>
/// Represents a class for LoggingCommandDecorator.
/// </summary>
public class LoggingCommandDecorator<TCommand, TReturn>(IAsyncCommand<TCommand, TReturn> decorated, ILogger log)
    : IAsyncCommand<TCommand, TReturn>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task<TReturn> ExecuteAsync(TCommand param)
    {
        var type = decorated.GetType();
        log.LogInformation($"starting Execute on {type}");
        var result = await decorated.ExecuteAsync(param);
        log.LogInformation($"finished Execute() on {type}");
        return result;
    }
}