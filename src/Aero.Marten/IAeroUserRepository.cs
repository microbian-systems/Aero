using Aero.Models.Entities;

namespace Aero.MartenDB;

public interface IAeroUserRepository
{
    IEnumerable<AeroUser> GetAll();
    Task<long> CountAsync();
    Task<bool> ExistsAsync(long id);
    Task<IEnumerable<AeroUser>> GetAllAsync();
    Task<Option<AeroUser>> GetByIdAsync(long id);
    Task<IEnumerable<AeroUser>> GetByIdsAsync(IEnumerable<long> ids);
    Option<AeroUser> FindById(long id);
    IEnumerable<AeroUser> Find(Expression<Func<AeroUser, bool>> predicate);
    Task<IEnumerable<AeroUser>> FindAsync(Expression<Func<AeroUser, bool>> predicate);
    Task<Option<AeroUser>> FindByIdAsync(long id);
    AeroUser Insert(AeroUser entity);
    AeroUser Update(AeroUser entity);
    AeroUser Upsert(AeroUser entity);
    bool Delete(long id);
    bool Delete(AeroUser entity);
    Task<AeroUser> InsertAsync(AeroUser entity);
    Task<AeroUser> UpdateAsync(AeroUser entity);
    Task<AeroUser> UpsertAsync(AeroUser entity);
    Task<bool> DeleteAsync(long id);
    Task<bool> DeleteAsync(AeroUser entity);
}