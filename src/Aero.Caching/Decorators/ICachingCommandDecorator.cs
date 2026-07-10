using Aero.Core.Commands;

namespace Aero.Caching.Decorators;

/// <summary>
/// Defines an interface for ICachingCommandDecoratorAsync.
/// </summary>
public interface ICachingCommandDecoratorAsync<T> : ICommandAsync<T>{}
/// <summary>
/// Defines an interface for ICachingCommandDecoratorSync.
/// </summary>
public interface ICachingCommandDecoratorSync<T> : ICommand<T>{}
    
    
/// <summary>
/// Defines an interface for ICachingCommandDecorator.
/// </summary>
public interface ICachingCommandDecorator<T> : ICachingCommandDecoratorSync<T>, ICachingCommandDecoratorAsync<T>
{
}