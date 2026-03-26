using Aero.Common.Commands;


namespace Aero.Marten;

public interface ISaveToDbCommand<T> : IAsyncCommand<T, T>
{
}
    
public interface ISaveToDbCommand<T, TKey> : IAsyncCommand<T, T> where T : IEntity<TKey> where TKey : IEquatable<TKey>
{
}