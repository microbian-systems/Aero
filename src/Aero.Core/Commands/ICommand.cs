namespace Aero.Core.Commands;
// todo - replace these commands with MediatR
// public interface ICommand
// {
//     void Execute();
//     void Execute<T>(T param);
//     Task ExecuteAsync();
//     Task ExecuteAsync<T>(T param);
// }
//
// public interface ICommand<T>
// {
//     T Execute();
//     T Execute<P>(P param);
//     Task<T> ExecuteAsync();
//     Task<T> ExecuteAsync<P>(P param);
// }

/// <summary>
/// Defines an interface for ICommandAsync.
/// </summary>
public interface ICommandAsync<T, TReturn>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
Task<TReturn> ExecuteAsync(T param);
}
    
/// <summary>
/// Defines an interface for ICommandAsync.
/// </summary>
public interface ICommandAsync<T>
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
Task ExecuteAsync(T param);
}
    
/// <summary>
/// Defines an interface for ICommandAsync.
/// </summary>
public interface ICommandAsync
{
        /// <summary>
    /// ExecuteAsync method.
    /// </summary>
Task ExecuteAsync();
}

/// <summary>
/// Defines an interface for ICommand.
/// </summary>
public interface ICommand<T> 
{
        /// <summary>
    /// Execute method.
    /// </summary>
void Execute(T param);
}

/// <summary>
/// Defines an interface for ICommand.
/// </summary>
public interface ICommand
{
        /// <summary>
    /// Execute method.
    /// </summary>
void Execute();
}
    
/// <summary>
/// Represents a param for the command pattern
/// </summary>
public interface ICommandParameter
{
}