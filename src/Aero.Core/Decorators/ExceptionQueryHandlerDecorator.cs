using Aero.Core.Patterns;

namespace Aero.Core.Decorators;

/// <summary>
/// Represents a class for ExceptionQueryHandlerDecorator.
/// </summary>
public class ExceptionQueryHandlerDecorator<TReturn>(IAsyncQueryHandler<TReturn> decorated, ILogger log)
    : IAsyncQueryHandler<TReturn>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
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
    
/// <summary>
/// Represents a class for ExceptionQueryHandlerDecorator.
/// </summary>
public class ExceptionQueryHandlerDecorator<TParam, TReturn>(IAsyncQueryHandler<TParam, TReturn> decorated, ILogger log)
    : IAsyncQueryHandler<TParam, TReturn>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
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