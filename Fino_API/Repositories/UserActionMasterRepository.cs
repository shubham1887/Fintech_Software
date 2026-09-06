using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserActionMasterRepository : IUserActionMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserActionMaster>> _listCache;
        public UserActionMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserActionMaster>>(); }
        public async Task<List<UserActionMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserActionMasters, () => _context.UserActionMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserActionMaster>();
        public async Task<UserActionMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserActionMasterID == id); }
        public async Task<List<UserActionMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserActionMaster entity) { await _context.UserActionMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserActionMasters); return result; }
        public async Task<int> Update(UserActionMaster entity) { _context.UserActionMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserActionMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserActionMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserActionMasters); return result; }
    }
}
