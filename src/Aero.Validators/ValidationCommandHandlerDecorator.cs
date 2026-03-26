using FluentValidation;
using Aero.Common.Commands;
using Microsoft.Extensions.Logging;

namespace Aero.Validators;

public class ValidationCommandHandlerDecorator<TCommand>(
    IValidator<TCommand> validator,
    IAsyncCommand<TCommand> handler,
    ILogger log)
    : IAsyncCommand<TCommand>
{
    // todo - investigate the following url for return async void as I'm doing here
    // https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
    public async Task ExecuteAsync(TCommand command) 
    {
        log.LogInformation($"entered {nameof(ValidationCommandHandlerDecorator<TCommand>)}");
        var res = await validator.ValidateAsync(command);
        if(!res.IsValid)
            throw new ValidationException($"validation exception has occurred for {nameof(command)}", res.Errors);
        await handler.ExecuteAsync(command);
    }
}
    
public class ValidationCommandHandlerDecorator<TCommand, TResult>(
    IValidator<TCommand> validator,
    IAsyncCommand<TCommand, TResult> handler,
    ILogger log)
    : IAsyncCommand<TCommand, TResult>
{
    // todo - investigate the following url for return async void as I'm doing here
    // https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
    public async Task<TResult> ExecuteAsync(TCommand command) 
    {
        log.LogInformation($"entered {nameof(ValidationCommandHandlerDecorator<TCommand, TResult>)}");
        var res = await validator.ValidateAsync(command);
        if(!res.IsValid)
            throw new ValidationException($"validation exception has occurred for {nameof(command)}", res.Errors);
        log.LogInformation($"validation succeeded in validation decorator for {typeof(TCommand)}");
        var results = await handler.ExecuteAsync(command);
        return results;
    }
}