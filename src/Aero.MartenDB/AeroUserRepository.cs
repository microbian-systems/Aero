using System.Linq.Expressions;
using Aero.Core.Railway;
using Aero.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Aero.MartenDB;

public class AeroUserRepository(IDocumentSession session, ILogger<AeroUserRepository> log)
    : RavenDbRepositoryBase<AeroUser>(session, log), IAeroUserRepository
{
    public override async Task<IEnumerable<AeroUser>> GetByIdsAsync(IEnumerable<string> ids)
    {
        throw new NotImplementedException();
    }

    public Option<AeroUser> FindById(string id)
    {
        throw new NotImplementedException();
    }

    public override async Task<IEnumerable<AeroUser>> FindAsync(Expression<Func<AeroUser, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Option<AeroUser> Insert(AeroUser entity)
    {
        throw new NotImplementedException();
    }

    public Option<AeroUser> Update(AeroUser entity)
    {
        throw new NotImplementedException();
    }

    public Option<AeroUser> Upsert(AeroUser entity)
    {
        throw new NotImplementedException();
    }
}