using Aero.Core.Commands;
using Polly;

namespace Aero.Core.Decorators;

// todo - replace polly with the microsoft.extensions.resilience package
/// <summary>
/// Represents a class for RetryCommandHandlerDecorator.
/// </summary>
public class RetryCommandHandlerDecorator<TCommand>(
    IAsyncCommand<TCommand> handler,
    ILogger<RetryCommandHandlerDecorator<TCommand>> log)
    : IAsyncCommand<TCommand>
{
    // todo - investigate the following url for return async void as I'm doing here
    // https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task ExecuteAsync(TCommand command)
    {
        log.LogInformation($"entered {nameof(RetryCommandHandlerDecorator<TCommand>)}");
        const int maxRetryAttempts = 5;
        var pauseBetweenFailures = TimeSpan.FromSeconds(2);

        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);

        await retryPolicy.ExecuteAsync(async () =>
        {
            await handler.ExecuteAsync(command);
        });
    }
}


/// <summary>
/// Represents a class for RetryCommandHandlerDecorator.
/// </summary>
public class RetryCommandHandlerDecorator<TCommand, TResult>(IAsyncCommand<TCommand, TResult> handler, ILogger log)
    : IAsyncCommand<TCommand, TResult>
{
    // todo - investigate the following url for return async void as I'm doing here
    // https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task<TResult> ExecuteAsync(TCommand command)
    {
        log.LogInformation($"entered {nameof(RetryCommandHandlerDecorator<TCommand>)}");
        const int maxRetryAttempts = 5;
        var pauseBetweenFailures = TimeSpan.FromSeconds(2);

        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);

        var results = default(TResult);
        await retryPolicy.ExecuteAsync(async () =>
        {
            results = await handler.ExecuteAsync(command);
        });
        return results;
    }
}