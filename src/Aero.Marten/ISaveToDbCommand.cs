using Aero.Core.Commands;

namespace Aero.Marten;

/// <summary>
/// Defines an interface for ISaveToDbCommand.
/// </summary>
public interface ISaveToDbCommand<T> : IAsyncCommand<T, T>
{
}
    
/// <summary>
/// Defines an interface for ISaveToDbCommand.
/// </summary>
public interface ISaveToDbCommand<T, TKey> : IAsyncCommand<T, T> where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
}