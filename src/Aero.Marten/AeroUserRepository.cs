using Aero.Models.Entities;

namespace Aero.Marten;

public class AeroUserRepository(IDocumentSession session, ILogger<AeroUserRepository> log)
    : AeroDbRepositoryBase<AeroUser>(session, log), IAeroUserRepository
{

}