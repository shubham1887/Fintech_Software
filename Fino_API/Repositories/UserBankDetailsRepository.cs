using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserBankDetailsRepository : IUserBankDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserBankDetails>> _listCache;
        public UserBankDetailsRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserBankDetails>>(); }
        public async Task<List<UserBankDetails>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserBankDetails, () => _context.UserBankDetails.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserBankDetails>();
        public async Task<UserBankDetails?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserBankDetailsID == id); }
        public async Task<List<UserBankDetails>> GetByUserID(int userID) { var list = await GetAll(); return list.Where(x => x.UserID == userID).ToList(); }
        public async Task<int> Insert(UserBankDetails entity) { await _context.UserBankDetails.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserBankDetails); return result; }
        public async Task<int> Update(UserBankDetails entity) { _context.UserBankDetails.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserBankDetails); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserBankDetails.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserBankDetails); return result; }
    }
}
