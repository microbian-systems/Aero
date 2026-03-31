using Aero.Models.Entities;

namespace Aero.MartenDB;

public class AeroUserRepository(IDocumentSession session, ILogger<AeroUserRepository> log)
    : AeroDbRepositoryBase<AeroUser>(session, log), IAeroUserRepository
{

}