using Aero.Models.Entities;

namespace Aero.MartenDB;

public class AeroUserRepository(IDocumentSession session, ILogger<AeroUserRepository> log)
    : AeroDbRepositoryBase<AeroUser>(session, log), IAeroUserRepository
{
    public override async Task<IEnumerable<AeroUser>> GetByIdsAsync(IEnumerable<long> ids)
    {
        throw new NotImplementedException();
    }

    public override Option<AeroUser> FindById(long id)
    {
        throw new NotImplementedException();
    }

    public override async Task<IEnumerable<AeroUser>> FindAsync(Expression<Func<AeroUser, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public override AeroUser Insert(AeroUser entity)
    {
        throw new NotImplementedException();
    }

    public override AeroUser Update(AeroUser entity)
    {
        throw new NotImplementedException();
    }

    public override AeroUser Upsert(AeroUser entity)
    {
        throw new NotImplementedException();
    }

    public override Task<AeroUser> InsertAsync(AeroUser entity)
    {
        throw new NotImplementedException();
    }

    public override Task<AeroUser> UpdateAsync(AeroUser entity)
    {
        throw new NotImplementedException();
    }

    public override Task<AeroUser> UpsertAsync(AeroUser entity)
    {
        throw new NotImplementedException();
    }
}