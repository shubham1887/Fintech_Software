using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class OperatorwiseCallbackSettingRepository : IOperatorwiseCallbackSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OperatorwiseCallbackSetting>> _listCache;

        public OperatorwiseCallbackSettingRepository(ApplicationDbContext context, MemoryCacheProcessing<List<OperatorwiseCallbackSetting>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<OperatorwiseCallbackSetting>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllOperatorwiseCallbackSettings, () =>
                _context.OperatorwiseCallbackSettings.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<OperatorwiseCallbackSetting>();
        }

        public async Task<List<OperatorwiseCallbackSetting>> GetByOperatorID(int operatorId)
        {
            var list = await GetAll();
            return list.Where(x => x.OperatorID == operatorId).ToList();
        }

        public async Task<OperatorwiseCallbackSetting?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.OperatorwiseCallbackSettingID == id);
        }

        public async Task<int> Insert(OperatorwiseCallbackSetting entity)
        {
            await _context.OperatorwiseCallbackSettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorwiseCallbackSettings);
            return result;
        }

        public async Task<int> Update(OperatorwiseCallbackSetting entity)
        {
            _context.OperatorwiseCallbackSettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorwiseCallbackSettings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.OperatorwiseCallbackSettings.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorwiseCallbackSettings);
            return result;
        }
    }
}
