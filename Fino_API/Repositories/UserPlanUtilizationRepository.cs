using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserPlanUtilizationRepository : IUserPlanUtilizationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserPlanUtilization>> _listCache;
        public UserPlanUtilizationRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserPlanUtilization>>(); }
        public async Task<List<UserPlanUtilization>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserPlanUtilizations, () => _context.UserPlanUtilizations.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserPlanUtilization>();
        public async Task<UserPlanUtilization?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserPlanUtilizationID == id); }
        public async Task<List<UserPlanUtilization>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserPlanUtilization entity) { await _context.UserPlanUtilizations.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserPlanUtilizations); return result; }
        public async Task<int> Update(UserPlanUtilization entity) { _context.UserPlanUtilizations.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserPlanUtilizations); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserPlanUtilizations.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserPlanUtilizations); return result; }
    }
}
