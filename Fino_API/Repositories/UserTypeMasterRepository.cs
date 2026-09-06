using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserTypeMasterRepository : IUserTypeMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserTypeMaster>> _listCache;

        public UserTypeMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<UserTypeMaster>>();
        }

        public async Task<List<UserTypeMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserTypes, () =>
                _context.UserTypeMasters.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.UserType).ToList()
            ) ?? new List<UserTypeMaster>();
        }

        public async Task<UserTypeMaster?> GetByID(int userTypeId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.UserTypeID == userTypeId);
        }

        public async Task<UserTypeMaster?> GetByName(string userType)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.UserType.Equals(userType, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> Insert(UserTypeMaster entity)
        {
            await _context.UserTypeMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserTypes);
            return result;
        }

        public async Task<int> Update(UserTypeMaster entity)
        {
            _context.UserTypeMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserTypes);
            return result;
        }

        public async Task<int> Delete(int userTypeId)
        {
            var entity = await _context.UserTypeMasters.FindAsync(userTypeId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserTypes);
            return result;
        }
    }
}
