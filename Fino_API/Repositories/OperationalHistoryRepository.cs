using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class OperationalHistoryRepository : IOperationalHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OperationalHistory>> _listCache;
        public OperationalHistoryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<OperationalHistory>>(); }
        public async Task<List<OperationalHistory>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllOperationalHistorys, () => _context.OperationalHistorys.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<OperationalHistory>();
        public async Task<OperationalHistory?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.OperationalHistoryID == id); }
        public async Task<List<OperationalHistory>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(OperationalHistory entity) { await _context.OperationalHistorys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperationalHistorys); return result; }
        public async Task<int> Update(OperationalHistory entity) { _context.OperationalHistorys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperationalHistorys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.OperationalHistorys.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperationalHistorys); return result; }
    }
}
