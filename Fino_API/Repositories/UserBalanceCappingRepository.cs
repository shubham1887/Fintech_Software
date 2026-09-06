using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserBalanceCappingRepository : IUserBalanceCappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserBalanceCapping>> _listCache;

        public UserBalanceCappingRepository(ApplicationDbContext context, MemoryCacheProcessing<List<UserBalanceCapping>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<UserBalanceCapping>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserBalanceCappings, () =>
                _context.UserBalanceCappings.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.UserMaster)
                    .Include(x => x.WalletMaster)
                    .ToList()
            ) ?? new List<UserBalanceCapping>();
        }

        public async Task<List<UserBalanceCapping>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserBalanceCapping?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserBalanceCappingID == id);
        }

        public async Task<int> Insert(UserBalanceCapping entity)
        {
            await _context.UserBalanceCappings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserBalanceCappings);
            return result;
        }

        public async Task<int> Update(UserBalanceCapping entity)
        {
            _context.UserBalanceCappings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserBalanceCappings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserBalanceCappings.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserBalanceCappings);
            return result;
        }
    }
}
