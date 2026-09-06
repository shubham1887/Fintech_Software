using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class TrustableDeviceRepository : ITrustableDeviceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<TrustableDevice>> _listCache;
        public TrustableDeviceRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<TrustableDevice>>(); }
        public async Task<List<TrustableDevice>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllTrustableDevices, () => _context.TrustableDevices.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<TrustableDevice>();
        public async Task<TrustableDevice?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ID == id); }
        public async Task<List<TrustableDevice>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(TrustableDevice entity) { await _context.TrustableDevices.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTrustableDevices); return result; }
        public async Task<int> Update(TrustableDevice entity) { _context.TrustableDevices.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTrustableDevices); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.TrustableDevices.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTrustableDevices); return result; }
        public async Task<TrustableDevice?> GetByUserAndBrowser(int userId, string browser)
        {
            var list = await GetByUserID(userId);
            return list.FirstOrDefault(x => x.Browser == browser && x.IsVerified);
        }
    }
}
