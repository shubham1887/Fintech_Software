using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class PayoutSettingRepository : IPayoutSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PayoutSetting>> _listCache;

        public PayoutSettingRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<PayoutSetting>>();
        }

        public async Task<List<PayoutSetting>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllPayoutSettings, () =>
                _context.PayoutSettings.AsNoTracking().ToList()
            ) ?? new List<PayoutSetting>();
        }

        public async Task<PayoutSetting?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.PayoutSettingID == id);
        }

        public async Task<int> Insert(PayoutSetting entity)
        {
            await _context.PayoutSettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPayoutSettings);
            return result;
        }

        public async Task<int> Update(PayoutSetting entity)
        {
            _context.PayoutSettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPayoutSettings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.PayoutSettings.FindAsync(id);
            if (entity == null) return 0;
            _context.PayoutSettings.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPayoutSettings);
            return result;
        }
    }
}
