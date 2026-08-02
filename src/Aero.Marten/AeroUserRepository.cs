using Aero.Models.Entities;

namespace Aero.Marten;

/// <summary>
/// Represents a class for AeroUserRepository.
/// </summary>
public class AeroUserRepository(IDocumentSession session, ILogger<AeroUserRepository> log)
    : AeroDbRepositoryBase<AeroUser>(session, log), IAeroUserRepository
{

}