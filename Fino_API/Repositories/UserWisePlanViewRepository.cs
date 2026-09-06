using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserWisePlanViewRepository : IUserWisePlanViewRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserWisePlanView>> _listCache;
        public UserWisePlanViewRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserWisePlanView>>(); }
        public async Task<List<UserWisePlanView>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserWisePlanViews, () => _context.UserWisePlanViews.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserWisePlanView>();
        public async Task<UserWisePlanView?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserWisePlanViewID == id); }
        public async Task<List<UserWisePlanView>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserWisePlanView entity) { await _context.UserWisePlanViews.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserWisePlanViews); return result; }
        public async Task<int> Update(UserWisePlanView entity) { _context.UserWisePlanViews.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserWisePlanViews); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserWisePlanViews.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserWisePlanViews); return result; }
    }
}
