using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class OTTSettingRepository : IOTTSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OTTSetting>> _listCache;
        public OTTSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<OTTSetting>>(); }
        public async Task<List<OTTSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllOTTSettings, () => _context.OTTSettings.AsNoTracking().ToList()) ?? new List<OTTSetting>();
        public async Task<OTTSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.OTTSettingID == id); }

        public async Task<int> Insert(OTTSetting entity) { await _context.OTTSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOTTSettings); return result; }
        public async Task<int> Update(OTTSetting entity) { _context.OTTSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOTTSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.OTTSettings.FindAsync(id); if (entity == null) return 0; _context.OTTSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOTTSettings); return result; }
    }
}
