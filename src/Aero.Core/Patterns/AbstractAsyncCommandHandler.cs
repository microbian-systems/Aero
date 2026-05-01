using Aero.Core.Commands;

namespace Aero.Core.Patterns;

public abstract class AbstractAsyncCommandHandler(ILogger<AbstractAsyncCommandHandler> log) : IAsyncCommand
{
    private readonly ILogger<AbstractAsyncCommandHandler> log = log;
    public abstract Task ExecuteAsync();
}
    
public abstract class AbstractAsyncCommandHandler<T>(ILogger<AbstractAsyncCommandHandler> log) : IAsyncCommand<T>
{
    private readonly ILogger<AbstractAsyncCommandHandler> log = log;
    public abstract Task ExecuteAsync(T param);
}
    
public abstract class AbstractAsyncCommandHandler<T, TReturn>(ILogger<AbstractAsyncCommandHandler> log)
    : IAsyncCommand<T, TReturn>
{
    private readonly ILogger<AbstractAsyncCommandHandler> log = log;

    public abstract Task<TReturn> ExecuteAsync(T param);
}