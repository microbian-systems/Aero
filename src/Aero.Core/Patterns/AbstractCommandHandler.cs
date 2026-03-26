using Aero.Common.Commands;

namespace Aero.Common.Patterns;

public abstract class AbstractCommandHandler(ILogger log) : ICommand
{
    private readonly ILogger log = log;
    public abstract void Execute();
}
    
public abstract class AbstractCommandHandler<T>(ILogger log) : ICommand<T>
{
    private readonly ILogger log = log;
    public abstract void Execute(T param);
    public void Execute(ICommandParameter param)
    {
        throw new System.NotImplementedException();
    }
}
    
public abstract class AbstractCommandHandler<T, TReturn>(ILogger log) : ICommand<T, TReturn>
{
    private readonly ILogger log = log;
    public abstract TReturn Execute(T param);
}