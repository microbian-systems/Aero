using Aero.Common.Commands;

namespace Aero.Common.Decorators;

public class LoggingCommandDecorator(IAsyncCommand decorated, ILogger log) : IAsyncCommand
{
    public async Task ExecuteAsync()
    {
        var type = decorated.GetType();
        log.LogInformation($"starting Execute on {type}");
        await decorated.ExecuteAsync();
        log.LogInformation($"finished Execute() on {type}");
    }
}
    
public class LoggingCommandDecorator<TCommand>(IAsyncCommand<TCommand> decorated, ILogger log) : IAsyncCommand<TCommand>
{
    public async Task ExecuteAsync(TCommand param)
    {
        var type = decorated.GetType();
        log.LogInformation($"starting Execute on {type}");
        await decorated.ExecuteAsync(param);
        log.LogInformation($"finished Execute() on {type}");
    }
}
    
public class LoggingCommandDecorator<TCommand, TReturn>(IAsyncCommand<TCommand, TReturn> decorated, ILogger log)
    : IAsyncCommand<TCommand, TReturn>
{
    public async Task<TReturn> ExecuteAsync(TCommand param)
    {
        var type = decorated.GetType();
        log.LogInformation($"starting Execute on {type}");
        var result = await decorated.ExecuteAsync(param);
        log.LogInformation($"finished Execute() on {type}");
        return result;
    }
}