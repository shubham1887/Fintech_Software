using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class SDKSettingRepository : ISDKSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SDKSetting>> _listCache;
        public SDKSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<SDKSetting>>(); }
        public async Task<List<SDKSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSDKSettings, () => _context.SDKSettings.AsNoTracking().ToList()) ?? new List<SDKSetting>();
        public async Task<SDKSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SDKSettingID == id); }

        public async Task<int> Insert(SDKSetting entity) { await _context.SDKSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSDKSettings); return result; }
        public async Task<int> Update(SDKSetting entity) { _context.SDKSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSDKSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.SDKSettings.FindAsync(id); if (entity == null) return 0; _context.SDKSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSDKSettings); return result; }
    }
}
