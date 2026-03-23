
namespace Aero.Common.Patterns;

public abstract class AbstractDecorator(ILogger log) : IDecorator
{
    private readonly ILogger log = log;
    public abstract void Execute();
}

public abstract class AbstractDecorator<T>(ILogger log) : IDecorator<T>
{
    private readonly ILogger log = log;
    public abstract void Execute(T param);
}
    
public abstract class AbstractDecorator<T, TReturn>(ILogger log) : IDecorator<T, TReturn>
{
    private readonly ILogger log = log;
    public abstract TReturn Execute(T param);
}