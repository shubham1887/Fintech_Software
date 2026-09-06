using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserOperatorLimitRepository : IUserOperatorLimitRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserOperatorLimit>> _listCache;

        public UserOperatorLimitRepository(ApplicationDbContext context, MemoryCacheProcessing<List<UserOperatorLimit>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<UserOperatorLimit>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserOperatorLimits, () =>
                _context.UserOperatorLimits.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.UserMaster)
                    .Include(x => x.OperatorMaster)
                    .Include(x => x.ServiceMaster)
                    .ToList()
            ) ?? new List<UserOperatorLimit>();
        }

        public async Task<List<UserOperatorLimit>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserOperatorLimit?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserOperatorLimitID == id);
        }

        public async Task<int> Insert(UserOperatorLimit entity)
        {
            await _context.UserOperatorLimits.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserOperatorLimits);
            return result;
        }

        public async Task<int> Update(UserOperatorLimit entity)
        {
            _context.UserOperatorLimits.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserOperatorLimits);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserOperatorLimits.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserOperatorLimits);
            return result;
        }
    }
}
