using Aero.Core.Commands;

namespace Aero.Marten;

/// <summary>
/// Defines an interface for IDynamicDbCachedQuery.
/// </summary>
public interface IDynamicDbCachedQuery<T> 
    : IAsyncCommand<Expression<Func<T, bool>>, IEnumerable<T>> 
    where T : class, IEntity<Guid>
{
}