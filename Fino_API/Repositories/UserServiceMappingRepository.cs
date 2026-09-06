using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserServiceMappingRepository : IUserServiceMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserServiceMapping>> _listCache;

        public UserServiceMappingRepository(ApplicationDbContext context, MemoryCacheProcessing<List<UserServiceMapping>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<UserServiceMapping>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserServiceMappings, () =>
                _context.UserServiceMappings.AsNoTracking()
                    .Include(x => x.UserMaster)
                    .Include(x => x.ServiceMaster)
                    .ToList()
            ) ?? new List<UserServiceMapping>();
        }

        public async Task<List<UserServiceMapping>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserServiceMapping?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserServiceMappingID == id);
        }

        public async Task<int> Insert(UserServiceMapping entity)
        {
            await _context.UserServiceMappings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserServiceMappings);
            return result;
        }

        public async Task<int> Update(UserServiceMapping entity)
        {
            // GetAll caches AsNoTracking rows with UserMaster / ServiceMaster loaded. Passing one of
            // those to _context.Update() marks the whole graph Modified (writing a stale UserMaster
            // row back) and lets relationship fixup overwrite the FKs. Copy the scalars onto the
            // tracked row instead so only this mapping's columns are written.
            var tracked = await _context.UserServiceMappings.FindAsync(entity.UserServiceMappingID);
            if (tracked == null)
                _context.UserServiceMappings.Update(entity);
            else
                _context.Entry(tracked).CurrentValues.SetValues(entity);

            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserServiceMappings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserServiceMappings.FindAsync(id);
            if (entity == null) return 0;
            _context.UserServiceMappings.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserServiceMappings);
            return result;
        }
    }
}
