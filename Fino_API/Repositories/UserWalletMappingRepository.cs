using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserWalletMappingRepository : IUserWalletMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserWalletMapping>> _listCache;

        public UserWalletMappingRepository(ApplicationDbContext context, MemoryCacheProcessing<List<UserWalletMapping>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<UserWalletMapping>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserWalletMappings, () =>
                _context.UserWalletMappings.AsNoTracking()
                    .Include(x => x.UserMaster)
                    .Include(x => x.WalletMaster)
                    .ToList()
            ) ?? new List<UserWalletMapping>();
        }

        public async Task<List<UserWalletMapping>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserWalletMapping?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserWalletMappingID == id);
        }

        public async Task<int> Insert(UserWalletMapping entity)
        {
            await _context.UserWalletMappings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserWalletMappings);
            return result;
        }

        public async Task<int> Update(UserWalletMapping entity)
        {
            _context.UserWalletMappings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserWalletMappings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserWalletMappings.FindAsync(id);
            if (entity == null) return 0;
            _context.UserWalletMappings.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserWalletMappings);
            return result;
        }
    }
}
