using Aero.Core.Commands;

namespace Aero.Core.Patterns;

/// <summary>
/// Represents a class for AbstractCommandHandler.
/// </summary>
public abstract class AbstractCommandHandler(ILogger log) : ICommand
{
    private readonly ILogger log = log;
        /// <summary>
    /// Execute method.
    /// </summary>
public abstract void Execute();
}
    
/// <summary>
/// Represents a class for AbstractCommandHandler.
/// </summary>
public abstract class AbstractCommandHandler<T>(ILogger log) : ICommand<T>
{
    private readonly ILogger log = log;
        /// <summary>
    /// Execute method.
    /// </summary>
public abstract void Execute(T param);
        /// <summary>
    /// Execute method.
    /// </summary>
public void Execute(ICommandParameter param)
    {
        throw new System.NotImplementedException();
    }
}
    
/// <summary>
/// Represents a class for AbstractCommandHandler.
/// </summary>
public abstract class AbstractCommandHandler<T, TReturn>(ILogger log) : ICommand<T, TReturn>
{
    private readonly ILogger log = log;
        /// <summary>
    /// Execute method.
    /// </summary>
public abstract TReturn Execute(T param);
}