using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class WrongPasswordHistoryRepository : IWrongPasswordHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<WrongPasswordHistory>> _listCache;
        public WrongPasswordHistoryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<WrongPasswordHistory>>(); }
        public async Task<List<WrongPasswordHistory>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllWrongPasswordHistorys, () => _context.WrongPasswordHistorys.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<WrongPasswordHistory>();
        public async Task<WrongPasswordHistory?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.WrongPasswordHistoryID == id); }
        public async Task<List<WrongPasswordHistory>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(WrongPasswordHistory entity) { await _context.WrongPasswordHistorys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllWrongPasswordHistorys); return result; }
        public async Task<int> Update(WrongPasswordHistory entity) { _context.WrongPasswordHistorys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllWrongPasswordHistorys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.WrongPasswordHistorys.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllWrongPasswordHistorys); return result; }
        public async Task<int> GetCountByUserID(int userId) { var list = await GetByUserID(userId); return list.Count; }
        public async Task<int> DeleteByUserID(int userId)
        {
            var items = _context.WrongPasswordHistorys.Where(x => x.UserID == userId && !x.IsDeleted);
            foreach (var e in items) { e.IsDeleted = true; e.UpdatedateTime = DateTime.UtcNow; }
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWrongPasswordHistorys);
            return result;
        }
    }
}
