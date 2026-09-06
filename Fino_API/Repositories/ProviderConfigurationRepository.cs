using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderConfigurationRepository : IProviderConfigurationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderConfiguration>> _listCache;

        public ProviderConfigurationRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<ProviderConfiguration>>();
        }

        public async Task<List<ProviderConfiguration>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllProviderConfigurations, () =>
                _context.ProviderConfigurations
                    .Include(x => x.OperatorMaster)
                    .Include(x => x.ProviderMaster)
                    .Include(x => x.ServiceMaster)
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<ProviderConfiguration>();
        }

        public async Task<List<ProviderConfiguration>> GetByProviderID(int providerId)
        {
            var all = await GetAll();
            return all.Where(x => x.ProviderID == providerId).ToList();
        }

        public async Task<List<ProviderConfiguration>> GetByOperatorID(int operatorId)
        {
            var all = await GetAll();
            return all.Where(x => x.OperatorID == operatorId).ToList();
        }

        public async Task<ProviderConfiguration?> GetByID(int configurationId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.ConfigurationID == configurationId);
        }

        public async Task<int> Insert(ProviderConfiguration entity)
        {
            await _context.ProviderConfigurations.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderConfigurations);
            return result;
        }

        public async Task<int> Update(ProviderConfiguration entity)
        {
            _context.ProviderConfigurations.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderConfigurations);
            return result;
        }

        public async Task<int> Delete(int configurationId)
        {
            var entity = await _context.ProviderConfigurations.FindAsync(configurationId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderConfigurations);
            return result;
        }
    }
}
