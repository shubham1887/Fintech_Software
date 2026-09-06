using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserPaymentCollectMappingRepository : IUserPaymentCollectMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserPaymentCollectMapping>> _listCache;
        public UserPaymentCollectMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserPaymentCollectMapping>>(); }
        public async Task<List<UserPaymentCollectMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserPaymentCollectMappings, () => _context.UserPaymentCollectMappings.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserPaymentCollectMapping>();
        public async Task<UserPaymentCollectMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserPaymentCollectMappingID == id); }
        public async Task<List<UserPaymentCollectMapping>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserPaymentCollectMapping entity) { await _context.UserPaymentCollectMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserPaymentCollectMappings); return result; }
        public async Task<int> Update(UserPaymentCollectMapping entity) { _context.UserPaymentCollectMappings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserPaymentCollectMappings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserPaymentCollectMappings.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserPaymentCollectMappings); return result; }
    }
}
