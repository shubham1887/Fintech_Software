using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class ServiceSubscriptionPlanDetailsRepository : IServiceSubscriptionPlanDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ServiceSubscriptionPlanDetails>> _listCache;
        public ServiceSubscriptionPlanDetailsRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ServiceSubscriptionPlanDetails>>(); }
        public async Task<List<ServiceSubscriptionPlanDetails>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllServiceSubscriptionPlanDetailss, () => _context.ServiceSubscriptionPlanDetailss.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ServiceSubscriptionPlanDetails>();
        public async Task<ServiceSubscriptionPlanDetails?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ServiceSubscriptionPlanDetailsID == id); }

        public async Task<int> Insert(ServiceSubscriptionPlanDetails entity) { await _context.ServiceSubscriptionPlanDetailss.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceSubscriptionPlanDetailss); return result; }
        public async Task<int> Update(ServiceSubscriptionPlanDetails entity) { _context.ServiceSubscriptionPlanDetailss.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceSubscriptionPlanDetailss); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ServiceSubscriptionPlanDetailss.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceSubscriptionPlanDetailss); return result; }
    }
}
