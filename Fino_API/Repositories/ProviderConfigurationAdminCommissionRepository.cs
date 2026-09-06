using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderConfigurationAdminCommissionRepository : IProviderConfigurationAdminCommissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderConfigurationAdminCommission>> _listCache;
        public ProviderConfigurationAdminCommissionRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ProviderConfigurationAdminCommission>>(); }
        public async Task<List<ProviderConfigurationAdminCommission>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllProviderConfigurationAdminCommissions, () => _context.ProviderConfigurationAdminCommissions.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ProviderConfigurationAdminCommission>();
        public async Task<ProviderConfigurationAdminCommission?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ProviderConfigurationAdminCommissionID == id); }
        public async Task<List<ProviderConfigurationAdminCommission>> GetByProviderID(int providerid) { var list = await GetAll(); return list.Where(x => x.ProviderID == providerid).ToList(); }
        public async Task<int> Insert(ProviderConfigurationAdminCommission entity) { await _context.ProviderConfigurationAdminCommissions.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderConfigurationAdminCommissions); return result; }
        public async Task<int> Update(ProviderConfigurationAdminCommission entity) { _context.ProviderConfigurationAdminCommissions.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderConfigurationAdminCommissions); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ProviderConfigurationAdminCommissions.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderConfigurationAdminCommissions); return result; }
    }
}
