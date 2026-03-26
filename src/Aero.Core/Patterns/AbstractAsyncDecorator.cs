namespace Aero.Common.Patterns;

public abstract class AbstractAsyncDecorator(ILogger log) : IAsyncDecorator
{
    private readonly ILogger log = log;

    public abstract Task ExecuteAsync();
}

public abstract class AbstractAsyncDecorator<T>(ILogger log) : IAsyncDecorator<T>
{
    private readonly ILogger log = log;

    public abstract Task ExecuteAsync(T parameter);
}
    
public abstract class AbstractAsyncDecorator<T, TResult>(ILogger log) : IAsyncDecorator<T, TResult>
{
    private readonly ILogger log = log;

    public abstract Task<TResult> ExecuteAsync(T parameter);
}