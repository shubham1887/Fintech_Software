using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserServiceChargeRepository : IUserServiceChargeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserServiceCharge>> _listCache;

        public UserServiceChargeRepository(ApplicationDbContext context, MemoryCacheProcessing<List<UserServiceCharge>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<UserServiceCharge>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserServiceCharges, () =>
                _context.UserServiceCharges.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<UserServiceCharge>();
        }

        public async Task<List<UserServiceCharge>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserServiceCharge?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserServiceChargeID == id);
        }

        public async Task<int> Insert(UserServiceCharge entity)
        {
            await _context.UserServiceCharges.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserServiceCharges);
            return result;
        }

        public async Task<int> Update(UserServiceCharge entity)
        {
            _context.UserServiceCharges.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserServiceCharges);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserServiceCharges.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserServiceCharges);
            return result;
        }
    }
}
