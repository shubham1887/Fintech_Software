using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserWisePlanLimitRepository : IUserWisePlanLimitRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserWisePlanLimit>> _listCache;
        public UserWisePlanLimitRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserWisePlanLimit>>(); }
        public async Task<List<UserWisePlanLimit>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserWisePlanLimits, () => _context.UserWisePlanLimits.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserWisePlanLimit>();
        public async Task<UserWisePlanLimit?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserWisePlanLimitID == id); }
        public async Task<List<UserWisePlanLimit>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserWisePlanLimit entity) { await _context.UserWisePlanLimits.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserWisePlanLimits); return result; }
        public async Task<int> Update(UserWisePlanLimit entity) { _context.UserWisePlanLimits.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserWisePlanLimits); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserWisePlanLimits.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserWisePlanLimits); return result; }
    }
}
