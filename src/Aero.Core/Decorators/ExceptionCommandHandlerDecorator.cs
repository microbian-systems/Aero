using Aero.Common.Commands;

namespace Aero.Common.Decorators;

public class ExceptionCommandHandlerDecorator(IAsyncCommand decorated, ILogger log) : IAsyncCommand
{
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
    
public class ExceptionCommandHandlerDecorator<TCommand>(IAsyncCommand<TCommand> decorated, ILogger log)
    : IAsyncCommand<TCommand>
{
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
    
public class ExceptionCommandHandlerDecorator<TCommand, TReturn>(
    IAsyncCommand<TCommand, TReturn> decorated,
    ILogger log) : IAsyncCommand<TCommand, TReturn>
{
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