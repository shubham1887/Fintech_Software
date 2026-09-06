using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<Notification>> _listCache;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<Notification>>();
        }

        public async Task<List<Notification>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllNotifications, () =>
                _context.Notifications
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<Notification>();
        }

        public async Task<Notification?> GetByID(int notificationId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.NotificationID == notificationId);
        }

        public async Task<int> Insert(Notification entity)
        {
            await _context.Notifications.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllNotifications);
            return result;
        }

        public async Task<int> Update(Notification entity)
        {
            _context.Notifications.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllNotifications);
            return result;
        }

        public async Task<int> Delete(int notificationId)
        {
            var entity = await _context.Notifications.FindAsync(notificationId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllNotifications);
            return result;
        }
    }
}
