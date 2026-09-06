using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderUserMappingRepository : IProviderUserMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderUserMapping>> _listCache;

        public ProviderUserMappingRepository(ApplicationDbContext context, MemoryCacheProcessing<List<ProviderUserMapping>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<ProviderUserMapping>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllProviderUserMappings, () =>
                _context.ProviderUserMappings.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.UserMaster)
                    .ToList()
            ) ?? new List<ProviderUserMapping>();
        }

        public async Task<List<ProviderUserMapping>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<ProviderUserMapping?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.ProviderUserMappingID == id);
        }

        public async Task<int> Insert(ProviderUserMapping entity)
        {
            await _context.ProviderUserMappings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderUserMappings);
            return result;
        }

        public async Task<int> Update(ProviderUserMapping entity)
        {
            _context.ProviderUserMappings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderUserMappings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.ProviderUserMappings.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderUserMappings);
            return result;
        }
    }
}
