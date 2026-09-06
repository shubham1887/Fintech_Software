using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ServiceSubscriptionPlanMasterRepository : IServiceSubscriptionPlanMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ServiceSubscriptionPlanMaster>> _listCache;
        public ServiceSubscriptionPlanMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ServiceSubscriptionPlanMaster>>(); }
        public async Task<List<ServiceSubscriptionPlanMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllServiceSubscriptionPlanMasters, () => _context.ServiceSubscriptionPlanMasters.AsNoTracking().ToList()) ?? new List<ServiceSubscriptionPlanMaster>();
        public async Task<ServiceSubscriptionPlanMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ServiceSubscriptionPlanMasterID == id); }

        public async Task<int> Insert(ServiceSubscriptionPlanMaster entity) { await _context.ServiceSubscriptionPlanMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceSubscriptionPlanMasters); return result; }
        public async Task<int> Update(ServiceSubscriptionPlanMaster entity) { _context.ServiceSubscriptionPlanMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceSubscriptionPlanMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ServiceSubscriptionPlanMasters.FindAsync(id); if (entity == null) return 0; _context.ServiceSubscriptionPlanMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllServiceSubscriptionPlanMasters); return result; }
    }
}
