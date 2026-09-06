using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class CMSSettingRepository : ICMSSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CMSSetting>> _listCache;
        public CMSSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CMSSetting>>(); }
        public async Task<List<CMSSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCMSSettings, () => _context.CMSSettings.AsNoTracking().ToList()) ?? new List<CMSSetting>();
        public async Task<CMSSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CMSSettingID == id); }

        public async Task<int> Insert(CMSSetting entity) { await _context.CMSSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCMSSettings); return result; }
        public async Task<int> Update(CMSSetting entity) { _context.CMSSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCMSSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CMSSettings.FindAsync(id); if (entity == null) return 0; _context.CMSSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCMSSettings); return result; }
    }
}
