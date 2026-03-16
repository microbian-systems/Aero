using Aero.Core.Data;

namespace Aero.MartenDB;

public interface IAeroDbUnitOfWork : IAsyncUnitOfWork
{
    // Add your repository property here
    IAeroUserRepository Users { get; }
    IDocumentSession Session { get; }
}