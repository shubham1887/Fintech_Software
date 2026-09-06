using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class EcollectionSettingRepository : IEcollectionSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<EcollectionSetting>> _listCache;
        public EcollectionSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<EcollectionSetting>>(); }
        public async Task<List<EcollectionSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllEcollectionSettings, () => _context.EcollectionSettings.AsNoTracking().ToList()) ?? new List<EcollectionSetting>();
        public async Task<EcollectionSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.EcollectionSettingID == id); }

        public async Task<int> Insert(EcollectionSetting entity) { await _context.EcollectionSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllEcollectionSettings); return result; }
        public async Task<int> Update(EcollectionSetting entity) { _context.EcollectionSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllEcollectionSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.EcollectionSettings.FindAsync(id); if (entity == null) return 0; _context.EcollectionSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllEcollectionSettings); return result; }
    }
}
