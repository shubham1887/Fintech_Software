using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserSubscriptionRepository : IUserSubscriptionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserSubscription>> _listCache;

        public UserSubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<UserSubscription>>();
        }

        public async Task<List<UserSubscription>> GetAll() =>
            _listCache.GetOrCreate(CacheKeys.AllUserSubscriptions,
                () => _context.UserSubscriptions.AsNoTracking().Where(x => !x.IsDeleted).ToList())
            ?? new List<UserSubscription>();

        public async Task<List<UserSubscription>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<int> Insert(UserSubscription entity)
        {
            await _context.UserSubscriptions.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserSubscriptions);
            return result;
        }

        public async Task<int> Update(UserSubscription entity)
        {
            _context.UserSubscriptions.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserSubscriptions);
            return result;
        }
    }
}
