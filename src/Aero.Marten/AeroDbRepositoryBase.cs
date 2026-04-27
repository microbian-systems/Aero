using static System.GC;


namespace Aero.Marten;

// todo - perform retry on martendb exeption connection type or similar 

public abstract class AeroDbRepositoryBase<TEntity>(
    IDocumentSession session,
    ILogger<AeroDbRepositoryBase<TEntity>> log)
    : MartenGenericRepositoryOption<TEntity>(session, log)
    where TEntity : ISnowflakeEntity, new()
{
    protected readonly IDocumentSession session = session;
      

    // todo - implement IAsyncDisposable and its pattern for AeroDbRepository
    public void Dispose()
    {
        session.Dispose();
        SuppressFinalize(this);
    }
}