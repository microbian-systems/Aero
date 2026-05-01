namespace Aero.Core.Commands;



/// <summary>
/// Command pattern to be used as a base interface for specific ICommandX interfaces (see remarks)
/// </summary>
/// <remarks>compatible with orleans serialization</remarks>
public interface IAsyncCommand 
{
    Task ExecuteAsync();
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T">Any type to be based to Execute method</typeparam>
/// <remarks>compatible with orleans serialization</remarks>
public interface IAsyncCommand<in T> 
{
    Task ExecuteAsync(T parameter);
}

/// <summary>
/// Command that takes a parameter and returns a value
/// </summary>
/// <typeparam name="T">Any type to be based to Execute method</typeparam>
/// <typeparam name="TReturn">Expected return value of type TReturn</typeparam>
/// <remarks>compatible with orleans serialization</remarks>
public interface IAsyncCommand<in T, TReturn> 
{
    Task<TReturn> ExecuteAsync(T parameter);
}

// public interface ICommand<in T>
// {
//     void Execute(T param);
// }

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TReturn"></typeparam>
/// <remarks>compatible with orleans serialization</remarks>
public interface ICommand<in T, out TReturn> 
{
    TReturn Execute(T param);
}