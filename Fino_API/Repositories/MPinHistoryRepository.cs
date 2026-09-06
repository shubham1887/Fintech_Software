using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class MPinHistoryRepository : IMPinHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<MPinHistory>> _listCache;
        public MPinHistoryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<MPinHistory>>(); }
        public async Task<List<MPinHistory>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllMPinHistorys, () => _context.MPinHistorys.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<MPinHistory>();
        public async Task<MPinHistory?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.MPinHistoryID == id); }
        public async Task<List<MPinHistory>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(MPinHistory entity) { await _context.MPinHistorys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMPinHistorys); return result; }
        public async Task<int> Update(MPinHistory entity) { _context.MPinHistorys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMPinHistorys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.MPinHistorys.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMPinHistorys); return result; }
    }
}
