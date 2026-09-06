using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserActionHistoryRepository : IUserActionHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserActionHistory>> _listCache;
        public UserActionHistoryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserActionHistory>>(); }
        public async Task<List<UserActionHistory>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserActionHistorys, () => _context.UserActionHistorys.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserActionHistory>();
        public async Task<UserActionHistory?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserHistoryID == id); }
        public async Task<List<UserActionHistory>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserActionHistory entity) { await _context.UserActionHistorys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserActionHistorys); return result; }
        public async Task<int> Update(UserActionHistory entity) { _context.UserActionHistorys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserActionHistorys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserActionHistorys.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserActionHistorys); return result; }
    }
}
