using static System.GC;


namespace Aero.Marten;

// todo - perform retry on martendb exeption connection type or similar 

/// <summary>
/// Represents a class for AeroDbRepositoryBase.
/// </summary>
public abstract class AeroDbRepositoryBase<TEntity>(
    IDocumentSession session,
    ILogger<AeroDbRepositoryBase<TEntity>> log)
    : MartenGenericRepositoryOption<TEntity>(session, log)
    where TEntity : ISnowflakeEntity, new()
{
        /// <summary>
    /// session.
    /// </summary>
protected readonly IDocumentSession session = session;
      

    // todo - implement IAsyncDisposable and its pattern for AeroDbRepository
        /// <summary>
    /// Dispose method.
    /// </summary>
public void Dispose()
    {
        session.Dispose();
        SuppressFinalize(this);
    }
}