
namespace Aero.Core.Patterns;

/// <summary>
/// Represents a class for AbstractDecorator.
/// </summary>
public abstract class AbstractDecorator(ILogger log) : IDecorator
{
    private readonly ILogger log = log;
        /// <summary>
    /// Execute method.
    /// </summary>
public abstract void Execute();
}

/// <summary>
/// Represents a class for AbstractDecorator.
/// </summary>
public abstract class AbstractDecorator<T>(ILogger log) : IDecorator<T>
{
    private readonly ILogger log = log;
        /// <summary>
    /// Execute method.
    /// </summary>
public abstract void Execute(T param);
}
    
/// <summary>
/// Represents a class for AbstractDecorator.
/// </summary>
public abstract class AbstractDecorator<T, TReturn>(ILogger log) : IDecorator<T, TReturn>
{
    private readonly ILogger log = log;
        /// <summary>
    /// Execute method.
    /// </summary>
public abstract TReturn Execute(T param);
}