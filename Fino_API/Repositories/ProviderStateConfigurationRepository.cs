using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderStateConfigurationRepository : IProviderStateConfigurationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderStateConfiguration>> _listCache;

        public ProviderStateConfigurationRepository(ApplicationDbContext context, MemoryCacheProcessing<List<ProviderStateConfiguration>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<ProviderStateConfiguration>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllProviderStateConfigurations, () =>
                _context.ProviderStateConfigurations.AsNoTracking()
                    .Include(x => x.ProviderMaster)
                    .ToList()
            ) ?? new List<ProviderStateConfiguration>();
        }

        public async Task<List<ProviderStateConfiguration>> GetByProviderID(int providerId)
        {
            var list = await GetAll();
            return list.Where(x => x.ProviderID == providerId).ToList();
        }

        public async Task<ProviderStateConfiguration?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.ProviderStateConfigurationID == id);
        }

        public async Task<int> Insert(ProviderStateConfiguration entity)
        {
            await _context.ProviderStateConfigurations.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderStateConfigurations);
            return result;
        }

        public async Task<int> Update(ProviderStateConfiguration entity)
        {
            _context.ProviderStateConfigurations.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderStateConfigurations);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.ProviderStateConfigurations.FindAsync(id);
            if (entity == null) return 0;
            _context.ProviderStateConfigurations.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderStateConfigurations);
            return result;
        }
    }
}
