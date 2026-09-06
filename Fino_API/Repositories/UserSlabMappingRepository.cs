using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserSlabMappingRepository : IUserSlabMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserSlabMapping>> _listCache;

        public UserSlabMappingRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<UserSlabMapping>>();
        }

        public async Task<List<UserSlabMapping>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserSlabMappings, () =>
                _context.UserSlabMappings
                    .Include(x => x.SlabMaster)
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<UserSlabMapping>();
        }

        public async Task<List<UserSlabMapping>> GetByUserID(int userId)
        {
            var all = await GetAll();
            return all.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserSlabMapping?> GetByID(int userSlabMappingId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.UserSlabMappingID == userSlabMappingId);
        }

        public async Task<int> Insert(UserSlabMapping entity)
        {
            await _context.UserSlabMappings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserSlabMappings);
            return result;
        }

        public async Task<int> Update(UserSlabMapping entity)
        {
            // GetByID returns a cached AsNoTracking entity with SlabMaster loaded. Attaching it via
            // _context.Update() lets EF's relationship fixup overwrite SlabMasterID back to the loaded
            // navigation's key (reverting the change). Load the tracked row and copy scalar values so
            // only the intended columns (UserID, SlabMasterID, dates) are updated.
            var tracked = await _context.UserSlabMappings.FindAsync(entity.UserSlabMappingID);
            if (tracked == null)
                _context.UserSlabMappings.Update(entity);
            else
                _context.Entry(tracked).CurrentValues.SetValues(entity);

            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserSlabMappings);
            return result;
        }

        public async Task<int> Delete(int userSlabMappingId)
        {
            var entity = await _context.UserSlabMappings.FindAsync(userSlabMappingId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserSlabMappings);
            return result;
        }
    }
}
