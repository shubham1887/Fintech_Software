using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserCreationSettingRepository : IUserCreationSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserCreationSetting>> _listCache;

        public UserCreationSettingRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<UserCreationSetting>>();
        }

        public async Task<List<UserCreationSetting>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserCreationSettings, () =>
                _context.UserCreationSettings.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<UserCreationSetting>();
        }

        public async Task<UserCreationSetting?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserCreationSettingID == id);
        }

        public async Task<List<UserCreationSetting>> GetByUserID(int userID)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userID).ToList();
        }

        public async Task<int> Insert(UserCreationSetting entity)
        {
            await _context.UserCreationSettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserCreationSettings);
            return result;
        }

        public async Task<int> Update(UserCreationSetting entity)
        {
            _context.UserCreationSettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserCreationSettings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserCreationSettings.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserCreationSettings);
            return result;
        }
    }
}
