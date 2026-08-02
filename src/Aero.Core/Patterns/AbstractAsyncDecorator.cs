namespace Aero.Core.Patterns;

/// <summary>
/// Represents a class for AbstractAsyncDecorator.
/// </summary>
public abstract class AbstractAsyncDecorator(ILogger log) : IAsyncDecorator
{
    private readonly ILogger log = log;

        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public abstract Task ExecuteAsync();
}

/// <summary>
/// Represents a class for AbstractAsyncDecorator.
/// </summary>
public abstract class AbstractAsyncDecorator<T>(ILogger log) : IAsyncDecorator<T>
{
    private readonly ILogger log = log;

        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public abstract Task ExecuteAsync(T parameter);
}
    
/// <summary>
/// Represents a class for AbstractAsyncDecorator.
/// </summary>
public abstract class AbstractAsyncDecorator<T, TResult>(ILogger log) : IAsyncDecorator<T, TResult>
{
    private readonly ILogger log = log;

        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public abstract Task<TResult> ExecuteAsync(T parameter);
}