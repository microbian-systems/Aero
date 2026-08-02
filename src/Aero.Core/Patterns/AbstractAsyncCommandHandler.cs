using Aero.Core.Commands;

namespace Aero.Core.Patterns;

/// <summary>
/// Represents a class for AbstractAsyncCommandHandler.
/// </summary>
public abstract class AbstractAsyncCommandHandler(ILogger<AbstractAsyncCommandHandler> log) : IAsyncCommand
{
    private readonly ILogger<AbstractAsyncCommandHandler> log = log;
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public abstract Task ExecuteAsync();
}
    
/// <summary>
/// Represents a class for AbstractAsyncCommandHandler.
/// </summary>
public abstract class AbstractAsyncCommandHandler<T>(ILogger<AbstractAsyncCommandHandler> log) : IAsyncCommand<T>
{
    private readonly ILogger<AbstractAsyncCommandHandler> log = log;
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public abstract Task ExecuteAsync(T param);
}
    
/// <summary>
/// Represents a class for AbstractAsyncCommandHandler.
/// </summary>
public abstract class AbstractAsyncCommandHandler<T, TReturn>(ILogger<AbstractAsyncCommandHandler> log)
    : IAsyncCommand<T, TReturn>
{
    private readonly ILogger<AbstractAsyncCommandHandler> log = log;

        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public abstract Task<TReturn> ExecuteAsync(T param);
}