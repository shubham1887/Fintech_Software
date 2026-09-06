using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class MicroATMSettingRepository : IMicroATMSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<MicroATMSetting>> _listCache;
        public MicroATMSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<MicroATMSetting>>(); }
        public async Task<List<MicroATMSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllMicroATMSettings, () => _context.MicroATMSettings.AsNoTracking().ToList()) ?? new List<MicroATMSetting>();
        public async Task<MicroATMSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.MicroATMSettingID == id); }

        public async Task<int> Insert(MicroATMSetting entity) { await _context.MicroATMSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMicroATMSettings); return result; }
        public async Task<int> Update(MicroATMSetting entity) { _context.MicroATMSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMicroATMSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.MicroATMSettings.FindAsync(id); if (entity == null) return 0; _context.MicroATMSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMicroATMSettings); return result; }
    }
}
