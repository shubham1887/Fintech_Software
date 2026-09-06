using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class DematSettingRepository : IDematSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<DematSetting>> _listCache;
        public DematSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<DematSetting>>(); }
        public async Task<List<DematSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllDematSettings, () => _context.DematSettings.AsNoTracking().ToList()) ?? new List<DematSetting>();
        public async Task<DematSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.DematSettingID == id); }

        public async Task<int> Insert(DematSetting entity) { await _context.DematSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDematSettings); return result; }
        public async Task<int> Update(DematSetting entity) { _context.DematSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDematSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.DematSettings.FindAsync(id); if (entity == null) return 0; _context.DematSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDematSettings); return result; }
    }
}
