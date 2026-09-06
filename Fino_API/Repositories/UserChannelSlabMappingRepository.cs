using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserChannelSlabMappingRepository : IUserChannelSlabMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserChannelSlabMapping>> _listCache;

        public UserChannelSlabMappingRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<UserChannelSlabMapping>>();
        }

        public async Task<List<UserChannelSlabMapping>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserChannelSlabMappings, () =>
                _context.UserChannelSlabMappings
                    .Include(x => x.ChannelSlabMaster)
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<UserChannelSlabMapping>();
        }

        public async Task<List<UserChannelSlabMapping>> GetByUserID(int userId)
        {
            var all = await GetAll();
            return all.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserChannelSlabMapping?> GetByID(int userChannelSlabMappingId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.UserChannelSlabMappingID == userChannelSlabMappingId);
        }

        public async Task<int> Insert(UserChannelSlabMapping entity)
        {
            await _context.UserChannelSlabMappings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserChannelSlabMappings);
            return result;
        }

        public async Task<int> Update(UserChannelSlabMapping entity)
        {
            _context.UserChannelSlabMappings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserChannelSlabMappings);
            return result;
        }

        public async Task<int> Delete(int userChannelSlabMappingId)
        {
            var entity = await _context.UserChannelSlabMappings.FindAsync(userChannelSlabMappingId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserChannelSlabMappings);
            return result;
        }
    }
}
