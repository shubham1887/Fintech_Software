using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class BulkPaySettingRepository : IBulkPaySettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<BulkPaySetting>> _listCache;
        public BulkPaySettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<BulkPaySetting>>(); }
        public async Task<List<BulkPaySetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllBulkPaySettings, () => _context.BulkPaySettings.AsNoTracking().ToList()) ?? new List<BulkPaySetting>();
        public async Task<BulkPaySetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.BulkPaySettingID == id); }

        public async Task<int> Insert(BulkPaySetting entity) { await _context.BulkPaySettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBulkPaySettings); return result; }
        public async Task<int> Update(BulkPaySetting entity) { _context.BulkPaySettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBulkPaySettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.BulkPaySettings.FindAsync(id); if (entity == null) return 0; _context.BulkPaySettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBulkPaySettings); return result; }
    }
}
