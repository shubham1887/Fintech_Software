using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class WebHookSettingRepository : IWebHookSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<WebHookSetting>> _listCache;

        public WebHookSettingRepository(ApplicationDbContext context, MemoryCacheProcessing<List<WebHookSetting>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<WebHookSetting>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllWebHookSettings, () =>
                _context.WebHookSettings.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<WebHookSetting>();
        }

        public async Task<WebHookSetting?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.WebHookSettingID == id);
        }

        public async Task<int> Insert(WebHookSetting entity)
        {
            await _context.WebHookSettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWebHookSettings);
            return result;
        }

        public async Task<int> Update(WebHookSetting entity)
        {
            _context.WebHookSettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWebHookSettings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.WebHookSettings.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWebHookSettings);
            return result;
        }
    }
}
