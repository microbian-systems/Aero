using Aero.Core.Commands;

namespace Aero.Core.Patterns;

/// <summary>
/// Defines an interface for IDecorator.
/// </summary>
public interface IDecorator : ICommand{}
/// <summary>
/// Defines an interface for IDecorator.
/// </summary>
public interface IDecorator<T> : ICommand<T>{}
/// <summary>
/// Defines an interface for IDecorator.
/// </summary>
public interface IDecorator<T, TReturn> : ICommand<T, TReturn>{}
    
/// <summary>
/// Defines an interface for IAsyncDecorator.
/// </summary>
public interface IAsyncDecorator : IAsyncCommand { }
/// <summary>
/// Defines an interface for IAsyncDecorator.
/// </summary>
public interface IAsyncDecorator<T> : IAsyncCommand<T>{}
/// <summary>
/// Defines an interface for IAsyncDecorator.
/// </summary>
public interface IAsyncDecorator<T, TResult> : IAsyncCommand<T, TResult>{}