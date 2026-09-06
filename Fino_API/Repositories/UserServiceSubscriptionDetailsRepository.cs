using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserServiceSubscriptionDetailsRepository : IUserServiceSubscriptionDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserServiceSubscriptionDetails>> _listCache;
        public UserServiceSubscriptionDetailsRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserServiceSubscriptionDetails>>(); }
        public async Task<List<UserServiceSubscriptionDetails>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserServiceSubscriptionDetailss, () => _context.UserServiceSubscriptionDetailss.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserServiceSubscriptionDetails>();
        public async Task<UserServiceSubscriptionDetails?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserServiceSubscriptionDetailsID == id); }
        public async Task<List<UserServiceSubscriptionDetails>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserServiceSubscriptionDetails entity) { await _context.UserServiceSubscriptionDetailss.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserServiceSubscriptionDetailss); return result; }
        public async Task<int> Update(UserServiceSubscriptionDetails entity) { _context.UserServiceSubscriptionDetailss.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserServiceSubscriptionDetailss); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserServiceSubscriptionDetailss.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserServiceSubscriptionDetailss); return result; }
    }
}
