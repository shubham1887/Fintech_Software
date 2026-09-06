using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserMerchantMasterRepository : IUserMerchantMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserMerchantMaster>> _listCache;
        public UserMerchantMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserMerchantMaster>>(); }
        public async Task<List<UserMerchantMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserMerchantMasters, () => _context.UserMerchantMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserMerchantMaster>();
        public async Task<UserMerchantMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserMerchantMasterID == id); }
        public async Task<List<UserMerchantMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserMerchantMaster entity) { await _context.UserMerchantMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserMerchantMasters); return result; }
        public async Task<int> Update(UserMerchantMaster entity) { _context.UserMerchantMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserMerchantMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserMerchantMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserMerchantMasters); return result; }
    }
}
