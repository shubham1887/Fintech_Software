using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserCreationWalletRepository : IUserCreationWalletRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserCreationWallet>> _listCache;

        public UserCreationWalletRepository(ApplicationDbContext context, MemoryCacheProcessing<List<UserCreationWallet>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<UserCreationWallet>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserCreationWallets, () =>
                _context.UserCreationWallets.AsNoTracking()
                    .Include(x => x.UserMaster)
                    .ToList()
            ) ?? new List<UserCreationWallet>();
        }

        public async Task<List<UserCreationWallet>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserCreationWallet?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserCreationWalletID == id);
        }

        public async Task<int> Insert(UserCreationWallet entity)
        {
            await _context.UserCreationWallets.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserCreationWallets);
            return result;
        }

        public async Task<int> Update(UserCreationWallet entity)
        {
            _context.UserCreationWallets.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserCreationWallets);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserCreationWallets.FindAsync(id);
            if (entity == null) return 0;
            _context.UserCreationWallets.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserCreationWallets);
            return result;
        }
    }
}
