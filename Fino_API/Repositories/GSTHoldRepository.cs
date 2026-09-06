using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class GSTHoldRepository : IGSTHoldRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<GSTHold>> _listCache;
        public GSTHoldRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<GSTHold>>(); }
        public async Task<List<GSTHold>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllGSTHolds, () => _context.GSTHolds.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<GSTHold>();
        public async Task<GSTHold?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.GSTHoldID == id); }
        public async Task<List<GSTHold>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(GSTHold entity) { await _context.GSTHolds.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllGSTHolds); return result; }
        public async Task<int> Update(GSTHold entity) { _context.GSTHolds.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllGSTHolds); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.GSTHolds.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllGSTHolds); return result; }
    }
}
