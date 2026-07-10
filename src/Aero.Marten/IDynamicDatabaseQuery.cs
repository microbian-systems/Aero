using Aero.Core.Commands;

namespace Aero.Marten;

/// <summary>
/// Defines an interface for IDynamicDatabaseQuery.
/// </summary>
public interface IDynamicDatabaseQuery<T> : IAsyncCommand<Expression<Func<T, bool>>, IEnumerable<T>> where T : class, IEntity<Guid>
{
}