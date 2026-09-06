using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserBalanceHistoryRepository : IUserBalanceHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserBalanceHistory>> _listCache;

        public UserBalanceHistoryRepository(ApplicationDbContext context, MemoryCacheProcessing<List<UserBalanceHistory>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<UserBalanceHistory>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserBalanceHistories, () =>
                _context.UserBalanceHistories.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<UserBalanceHistory>();
        }

        public async Task<List<UserBalanceHistory>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserBalanceHistory?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserBalanceHistoryID == id);
        }

        public async Task<int> Insert(UserBalanceHistory entity)
        {
            await _context.UserBalanceHistories.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserBalanceHistories);
            return result;
        }

        public async Task<int> Update(UserBalanceHistory entity)
        {
            _context.UserBalanceHistories.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserBalanceHistories);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserBalanceHistories.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserBalanceHistories);
            return result;
        }
    }
}
