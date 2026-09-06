using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class POSSettingMasterRepository : IPOSSettingMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<POSSettingMaster>> _listCache;
        public POSSettingMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<POSSettingMaster>>(); }
        public async Task<List<POSSettingMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPOSSettingMasters, () => _context.POSSettingMasters.AsNoTracking().ToList()) ?? new List<POSSettingMaster>();
        public async Task<POSSettingMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.POSSettingMasterID == id); }

        public async Task<int> Insert(POSSettingMaster entity) { await _context.POSSettingMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPOSSettingMasters); return result; }
        public async Task<int> Update(POSSettingMaster entity) { _context.POSSettingMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPOSSettingMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.POSSettingMasters.FindAsync(id); if (entity == null) return 0; _context.POSSettingMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPOSSettingMasters); return result; }
    }
}
