using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class LowBalanceConfigurationRepository : ILowBalanceConfigurationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<LowBalanceConfiguration>> _listCache;

        public LowBalanceConfigurationRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<LowBalanceConfiguration>>();
        }

        public async Task<List<LowBalanceConfiguration>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllLowBalanceConfigurations, () =>
                _context.LowBalanceConfigurations.AsNoTracking().Where(x => !x.IsDeleted).ToList()
            ) ?? new List<LowBalanceConfiguration>();
        }

        public async Task<List<LowBalanceConfiguration>> GetByUserID(int userID)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userID).ToList();
        }

        public async Task<LowBalanceConfiguration?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.LowBalanceConfigurationID == id);
        }

        public async Task<int> Insert(LowBalanceConfiguration entity)
        {
            await _context.LowBalanceConfigurations.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllLowBalanceConfigurations);
            return result;
        }

        public async Task<int> Update(LowBalanceConfiguration entity)
        {
            _context.LowBalanceConfigurations.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllLowBalanceConfigurations);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.LowBalanceConfigurations.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllLowBalanceConfigurations);
            return result;
        }
    }
}
