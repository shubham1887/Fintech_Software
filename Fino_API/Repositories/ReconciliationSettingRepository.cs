using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ReconciliationSettingRepository : IReconciliationSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ReconciliationSetting>> _listCache;

        public ReconciliationSettingRepository(ApplicationDbContext context, MemoryCacheProcessing<List<ReconciliationSetting>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<ReconciliationSetting>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllReconciliationSettings, () =>
                _context.ReconciliationSettings.AsNoTracking()
                    .Include(x => x.ProviderMaster)
                    .ToList()
            ) ?? new List<ReconciliationSetting>();
        }

        public async Task<List<ReconciliationSetting>> GetByProviderID(int providerId)
        {
            var list = await GetAll();
            return list.Where(x => x.ProviderID == providerId).ToList();
        }

        public async Task<ReconciliationSetting?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.ReconciliationSettingID == id);
        }

        public async Task<int> Insert(ReconciliationSetting entity)
        {
            await _context.ReconciliationSettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllReconciliationSettings);
            return result;
        }

        public async Task<int> Update(ReconciliationSetting entity)
        {
            _context.ReconciliationSettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllReconciliationSettings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.ReconciliationSettings.FindAsync(id);
            if (entity == null) return 0;
            _context.ReconciliationSettings.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllReconciliationSettings);
            return result;
        }
    }
}
