using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class ServiceSubscriptionCreatePlanMasterRepository : IServiceSubscriptionCreatePlanMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ServiceSubscriptionCreatePlanMaster>> _listCache;
        public ServiceSubscriptionCreatePlanMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ServiceSubscriptionCreatePlanMaster>>(); }
        public async Task<List<ServiceSubscriptionCreatePlanMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllServiceSubscriptionCreatePlanMasters, () => _context.ServiceSubscriptionCreatePlanMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ServiceSubscriptionCreatePlanMaster>();
        public async Task<ServiceSubscriptionCreatePlanMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ServiceSubscriptionCreatePlanMasterID == id); }

        public async Task<int> Insert(ServiceSubscriptionCreatePlanMaster entity) { await _context.ServiceSubscriptionCreatePlanMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceSubscriptionCreatePlanMasters); return result; }
        public async Task<int> Update(ServiceSubscriptionCreatePlanMaster entity) { _context.ServiceSubscriptionCreatePlanMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceSubscriptionCreatePlanMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ServiceSubscriptionCreatePlanMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceSubscriptionCreatePlanMasters); return result; }
    }
}
