using Aero.Common.Commands;

namespace Aero.Common.Decorators;

public class CpuBoundCommandHandlerDecorator<TCommand>(Func<IAsyncCommand<TCommand>> decorateeFactory, ILogger log)
    : IAsyncCommand<TCommand>
{
    // todo - investigate the following url for return async void as I'm doing here
    // https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
    public async Task ExecuteAsync(TCommand command) => await Task.Run(() =>
    {
        log.LogInformation($"entered {nameof(CpuBoundCommandHandlerDecorator<TCommand>)}");
        var cmd = command; 
        // execute on new thread & create new handler in this thread.
        var handler = decorateeFactory.Invoke();
        handler.ExecuteAsync(cmd);
    });
}