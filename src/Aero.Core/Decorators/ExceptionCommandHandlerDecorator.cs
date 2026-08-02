using Aero.Core.Commands;

namespace Aero.Core.Decorators;

/// <summary>
/// Represents a class for ExceptionCommandHandlerDecorator.
/// </summary>
public class ExceptionCommandHandlerDecorator(IAsyncCommand decorated, ILogger log) : IAsyncCommand
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task ExecuteAsync()
    {
        try
        {
            await decorated.ExecuteAsync();
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"ExceptionCommandHandlerDecorator caught {ex.GetType()} - {ex.Message}");
        }
    }
}
    
/// <summary>
/// Represents a class for ExceptionCommandHandlerDecorator.
/// </summary>
public class ExceptionCommandHandlerDecorator<TCommand>(IAsyncCommand<TCommand> decorated, ILogger log)
    : IAsyncCommand<TCommand>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task ExecuteAsync(TCommand param)
    {
        try
        {
            log.LogInformation($"executing ExceptionHandlerDecorator");
            await decorated.ExecuteAsync(param);
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"ExceptionCommandHandlerDecorator caught {ex.GetType()} - {ex.Message}");
        }
    }
}
    
/// <summary>
/// Represents a class for ExceptionCommandHandlerDecorator.
/// </summary>
public class ExceptionCommandHandlerDecorator<TCommand, TReturn>(
    IAsyncCommand<TCommand, TReturn> decorated,
    ILogger log) : IAsyncCommand<TCommand, TReturn>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task<TReturn> ExecuteAsync(TCommand param)
    {
        var result = default(TReturn);
        try
        {
            log.LogInformation($"executing ExceptionHandlerDecorator");
            result = await decorated.ExecuteAsync(param);
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"ExceptionCommandHandlerDecorator caught {ex.GetType()} - {ex.Message}");
        }

        return result;
    }
}