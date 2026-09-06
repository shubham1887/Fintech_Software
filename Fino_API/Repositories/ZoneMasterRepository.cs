using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ZoneMasterRepository : IZoneMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ZoneMaster>> _listCache;

        public ZoneMasterRepository(ApplicationDbContext context, MemoryCacheProcessing<List<ZoneMaster>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<ZoneMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllZoneMasters, () =>
                _context.ZoneMasters.AsNoTracking().ToList()
            ) ?? new List<ZoneMaster>();
        }

        public async Task<ZoneMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.ZoneMasterID == id);
        }

        public async Task<int> Insert(ZoneMaster entity)
        {
            await _context.ZoneMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllZoneMasters);
            return result;
        }

        public async Task<int> Update(ZoneMaster entity)
        {
            _context.ZoneMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllZoneMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.ZoneMasters.FindAsync(id);
            if (entity == null) return 0;
            _context.ZoneMasters.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllZoneMasters);
            return result;
        }
    }
}
