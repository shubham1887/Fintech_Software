using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class DeviceWiseFailLoginHistoryRepository : IDeviceWiseFailLoginHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<DeviceWiseFailLoginHistory>> _listCache;
        public DeviceWiseFailLoginHistoryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<DeviceWiseFailLoginHistory>>(); }
        public async Task<List<DeviceWiseFailLoginHistory>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllDeviceWiseFailLoginHistorys, () => _context.DeviceWiseFailLoginHistorys.AsNoTracking().ToList()) ?? new List<DeviceWiseFailLoginHistory>();
        public async Task<DeviceWiseFailLoginHistory?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.DeviceWiseFailLoginHistoryID == id); }

        public async Task<int> Insert(DeviceWiseFailLoginHistory entity) { await _context.DeviceWiseFailLoginHistorys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDeviceWiseFailLoginHistorys); return result; }
        public async Task<int> Update(DeviceWiseFailLoginHistory entity) { _context.DeviceWiseFailLoginHistorys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDeviceWiseFailLoginHistorys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.DeviceWiseFailLoginHistorys.FindAsync(id); if (entity == null) return 0; _context.DeviceWiseFailLoginHistorys.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDeviceWiseFailLoginHistorys); return result; }
        public async Task<int> GetTodayFailCount(string userName, string deviceId)
        {
            var list = await GetAll();
            return list.Count(x => x.UserName == userName && x.DeviceID == deviceId);
        }
    }
}
