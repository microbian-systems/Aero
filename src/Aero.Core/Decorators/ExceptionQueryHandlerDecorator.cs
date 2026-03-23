using Aero.Common.Patterns;

namespace Aero.Common.Decorators;

public class ExceptionQueryHandlerDecorator<TReturn>(IAsyncQueryHandler<TReturn> decorated, ILogger log)
    : IAsyncQueryHandler<TReturn>
{
    public async Task<TReturn> ExecuteAsync()
    {
        var result = default(TReturn);
        try
        {
            result = await decorated.ExecuteAsync();
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"ExceptionCommandHandlerDecorator caught {ex.GetType()} - {ex.Message}");
        }

        return result;
    }
}
    
public class ExceptionQueryHandlerDecorator<TParam, TReturn>(IAsyncQueryHandler<TParam, TReturn> decorated, ILogger log)
    : IAsyncQueryHandler<TParam, TReturn>
{
    public async Task<TReturn> ExecuteAsync(TParam param)
    {
        var result = default(TReturn);
        try
        {
            result = await decorated.ExecuteAsync(param);
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"ExceptionCommandHandlerDecorator caught {ex.GetType()} - {ex.Message}");
        }

        return result;
    }
}