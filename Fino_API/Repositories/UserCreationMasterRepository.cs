using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserCreationMasterRepository : IUserCreationMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserCreationMaster>> _listCache;

        public UserCreationMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<UserCreationMaster>>();
        }

        public async Task<List<UserCreationMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserCreationMasters, () =>
                _context.UserCreationMasters.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.ServiceMaster)
                    .ToList()
            ) ?? new List<UserCreationMaster>();
        }

        public async Task<UserCreationMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserCreationMasterID == id);
        }

        public async Task<List<UserCreationMaster>> GetByUserID(int userID)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userID).ToList();
        }

        public async Task<int> Insert(UserCreationMaster entity)
        {
            await _context.UserCreationMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserCreationMasters);
            return result;
        }

        public async Task<int> Update(UserCreationMaster entity)
        {
            _context.UserCreationMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserCreationMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserCreationMasters.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserCreationMasters);
            return result;
        }
    }
}
