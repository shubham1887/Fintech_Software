using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class FingerPrintDeviceTypeRepository : IFingerPrintDeviceTypeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<FingerPrintDeviceType>> _listCache;
        public FingerPrintDeviceTypeRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<FingerPrintDeviceType>>(); }
        public async Task<List<FingerPrintDeviceType>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllFingerPrintDeviceTypes, () => _context.FingerPrintDeviceTypes.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<FingerPrintDeviceType>();
        public async Task<FingerPrintDeviceType?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.FingerPrintDeviceTypeID == id); }

        public async Task<int> Insert(FingerPrintDeviceType entity) { await _context.FingerPrintDeviceTypes.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllFingerPrintDeviceTypes); return result; }
        public async Task<int> Update(FingerPrintDeviceType entity) { _context.FingerPrintDeviceTypes.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllFingerPrintDeviceTypes); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.FingerPrintDeviceTypes.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllFingerPrintDeviceTypes); return result; }
    }
}
