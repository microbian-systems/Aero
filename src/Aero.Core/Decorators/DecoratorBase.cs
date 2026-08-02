using Aero.Core.Commands;

namespace Aero.Core.Decorators;

/// <summary>
/// Represents a class for DecoratorBaseAsync.
/// </summary>
public abstract class DecoratorBaseAsync<T>(ICommandAsync<T> cmd, ILogger<DecoratorBaseAsync<T>> log) : ICommandAsync<T>
{
        /// <summary>
    /// cmd.
    /// </summary>
protected readonly ICommandAsync<T> cmd = cmd;
        /// <summary>
    /// log.
    /// </summary>
protected readonly ILogger<DecoratorBaseAsync<T>> log = log;

        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public abstract Task ExecuteAsync(T parameter);
}

/// <summary>
/// Represents a class for DecoratorBase.
/// </summary>
public abstract class DecoratorBase<T, TReturn>(ICommandAsync<T, TReturn> cmd, ILogger<DecoratorBaseAsync<T>> log)
    : ICommandAsync<T, TReturn>
    where T : ICommandParameter
{
    private readonly ICommandAsync<T, TReturn> cmd = cmd;

        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public abstract Task<TReturn> ExecuteAsync(T param);
        
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public virtual async Task<TReturn> ExecuteAsync(Func<T, Task<TReturn>> func, T parameter)
    {
        log.LogInformation($"wrapping {typeof(T)} through the Func<T> decorator");
        var result = await func(parameter);
        log.LogInformation($"successfuly wrapped {typeof(T)}");
        return result;
    }
}