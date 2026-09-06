using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserWisePayementGatewaySettingRepository : IUserWisePayementGatewaySettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserWisePayementGatewaySetting>> _listCache;
        public UserWisePayementGatewaySettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserWisePayementGatewaySetting>>(); }
        public async Task<List<UserWisePayementGatewaySetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserWisePayementGatewaySettings, () => _context.UserWisePayementGatewaySettings.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserWisePayementGatewaySetting>();
        public async Task<UserWisePayementGatewaySetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserWisePayementGatewaySettingID == id); }
        public async Task<List<UserWisePayementGatewaySetting>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserWisePayementGatewaySetting entity) { await _context.UserWisePayementGatewaySettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserWisePayementGatewaySettings); return result; }
        public async Task<int> Update(UserWisePayementGatewaySetting entity) { _context.UserWisePayementGatewaySettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserWisePayementGatewaySettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserWisePayementGatewaySettings.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserWisePayementGatewaySettings); return result; }
    }
}
