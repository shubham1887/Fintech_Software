using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class WebHookLogRepository : IWebHookLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<WebHookLog>> _listCache;

        public WebHookLogRepository(ApplicationDbContext context, MemoryCacheProcessing<List<WebHookLog>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<WebHookLog>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllWebHookLogs, () =>
                _context.WebHookLogs.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<WebHookLog>();
        }

        public async Task<WebHookLog?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.WebHookLogID == id);
        }

        public async Task<int> Insert(WebHookLog entity)
        {
            await _context.WebHookLogs.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWebHookLogs);
            return result;
        }

        public async Task<int> Update(WebHookLog entity)
        {
            _context.WebHookLogs.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWebHookLogs);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.WebHookLogs.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWebHookLogs);
            return result;
        }
    }
}
