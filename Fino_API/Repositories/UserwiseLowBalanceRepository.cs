using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserwiseLowBalanceRepository : IUserwiseLowBalanceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserwiseLowBalance>> _listCache;

        public UserwiseLowBalanceRepository(ApplicationDbContext context, MemoryCacheProcessing<List<UserwiseLowBalance>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<UserwiseLowBalance>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserwiseLowBalances, () =>
                _context.UserwiseLowBalances.AsNoTracking()
                    .Include(x => x.UserMaster)
                    .Include(x => x.WalletMaster)
                    .ToList()
            ) ?? new List<UserwiseLowBalance>();
        }

        public async Task<List<UserwiseLowBalance>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserwiseLowBalance?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserwiseLowBalanceID == id);
        }

        public async Task<int> Insert(UserwiseLowBalance entity)
        {
            await _context.UserwiseLowBalances.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseLowBalances);
            return result;
        }

        public async Task<int> Update(UserwiseLowBalance entity)
        {
            _context.UserwiseLowBalances.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseLowBalances);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserwiseLowBalances.FindAsync(id);
            if (entity == null) return 0;
            _context.UserwiseLowBalances.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseLowBalances);
            return result;
        }
    }
}
