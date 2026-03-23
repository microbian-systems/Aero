using Aero.Common.Commands;


namespace Aero.Marten;

public interface IDynamicDbCachedQuery<T> 
    : IAsyncCommand<Expression<Func<T, bool>>, IEnumerable<T>> 
    where T : class, IEntity<Guid>
{
}