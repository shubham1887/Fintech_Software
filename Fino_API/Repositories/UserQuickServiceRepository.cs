using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserQuickServiceRepository : IUserQuickServiceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserQuickService>> _listCache;

        public UserQuickServiceRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<UserQuickService>>();
        }

        private List<UserQuickService> All() =>
            _listCache.GetOrCreate(CacheKeys.AllUserQuickServices,
                () => _context.UserQuickServices.AsNoTracking().Where(x => !x.IsDeleted).ToList())
            ?? new List<UserQuickService>();

        public Task<List<UserQuickService>> GetByUserID(int userId) =>
            Task.FromResult(All().Where(x => x.UserID == userId).OrderBy(x => x.UISequence).ToList());

        public async Task<int> ReplaceForUser(int userId, IReadOnlyList<int> serviceIds)
        {
            // Hard-delete rather than soft-delete: the unique (UserID, ServiceID) index does not
            // know about IsDeleted, so a soft-deleted row would block the user from ever pinning
            // that service again. There is nothing to audit in a shortcut list anyway.
            var existing = await _context.UserQuickServices.Where(x => x.UserID == userId).ToListAsync();
            if (existing.Count > 0) _context.UserQuickServices.RemoveRange(existing);

            // Distinct keeps the index safe if a client sends the same id twice; the first
            // occurrence wins, so the order the user dragged them into is preserved.
            int sequence = 1;
            foreach (int serviceId in serviceIds.Where(id => id > 0).Distinct())
            {
                await _context.UserQuickServices.AddAsync(new UserQuickService
                {
                    UserID = userId,
                    ServiceID = serviceId,
                    UISequence = sequence++,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                });
            }

            int result = await _context.SaveChangesAsync();
            _listCache.Remove(CacheKeys.AllUserQuickServices);
            return result;
        }
    }
}
