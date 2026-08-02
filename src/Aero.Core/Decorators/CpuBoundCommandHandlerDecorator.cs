using Aero.Core.Commands;

namespace Aero.Core.Decorators;

/// <summary>
/// Represents a class for CpuBoundCommandHandlerDecorator.
/// </summary>
public class CpuBoundCommandHandlerDecorator<TCommand>(Func<IAsyncCommand<TCommand>> decorateeFactory, ILogger log)
    : IAsyncCommand<TCommand>
{
    // todo - investigate the following url for return async void as I'm doing here
    // https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
public async Task ExecuteAsync(TCommand command) => await Task.Run(() =>
    {
        log.LogInformation($"entered {nameof(CpuBoundCommandHandlerDecorator<TCommand>)}");
        var cmd = command; 
        // execute on new thread & create new handler in this thread.
        var handler = decorateeFactory.Invoke();
        handler.ExecuteAsync(cmd);
    });
}