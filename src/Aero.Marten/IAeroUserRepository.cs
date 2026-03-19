using Aero.Models.Entities;

namespace Aero.MartenDB;

public interface IAeroUserRepository
{
    IEnumerable<AeroUser> GetAll();
    Task<long> CountAsync();
    Task<bool> ExistsAsync(ulong id);
    Task<IEnumerable<AeroUser>> GetAllAsync();
    Task<Option<AeroUser>> GetByIdAsync(ulong id);
    Task<IEnumerable<AeroUser>> GetByIdsAsync(IEnumerable<ulong> ids);
    Option<AeroUser> FindById(ulong id);
    IEnumerable<AeroUser> Find(Expression<Func<AeroUser, bool>> predicate);
    Task<IEnumerable<AeroUser>> FindAsync(Expression<Func<AeroUser, bool>> predicate);
    Task<Option<AeroUser>> FindByIdAsync(ulong id);
    AeroUser Insert(AeroUser entity);
    AeroUser Update(AeroUser entity);
    AeroUser Upsert(AeroUser entity);
    bool Delete(ulong id);
    bool Delete(AeroUser entity);
    Task<AeroUser> InsertAsync(AeroUser entity);
    Task<AeroUser> UpdateAsync(AeroUser entity);
    Task<AeroUser> UpsertAsync(AeroUser entity);
    Task<bool> DeleteAsync(ulong id);
    Task<bool> DeleteAsync(AeroUser entity);
}