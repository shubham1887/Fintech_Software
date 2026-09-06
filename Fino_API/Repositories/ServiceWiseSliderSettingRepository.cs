using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ServiceWiseSliderSettingRepository : IServiceWiseSliderSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ServiceWiseSliderSetting>> _listCache;
        public ServiceWiseSliderSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ServiceWiseSliderSetting>>(); }
        public async Task<List<ServiceWiseSliderSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllServiceWiseSliderSettings, () => _context.ServiceWiseSliderSettings.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ServiceWiseSliderSetting>();
        public async Task<ServiceWiseSliderSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ServiceWiseSliderSettingID == id); }
        public async Task<List<ServiceWiseSliderSetting>> GetByServiceID(int serviceid) { var list = await GetAll(); return list.Where(x => x.ServiceID == serviceid).ToList(); }
        public async Task<int> Insert(ServiceWiseSliderSetting entity) { await _context.ServiceWiseSliderSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceWiseSliderSettings); return result; }
        public async Task<int> Update(ServiceWiseSliderSetting entity) { _context.ServiceWiseSliderSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceWiseSliderSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ServiceWiseSliderSettings.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceWiseSliderSettings); return result; }
    }
}
