using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class SlabMasterRepository : ISlabMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SlabMaster>> _listCache;

        public SlabMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<SlabMaster>>();
        }

        public async Task<List<SlabMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllSlabMasters, () =>
                _context.SlabMasters.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.SlabName).ToList()
            ) ?? new List<SlabMaster>();
        }

        public async Task<SlabMaster?> GetByID(int slabMasterId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.SlabMasterID == slabMasterId);
        }

        public async Task<SlabMaster?> GetByName(string slabName)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.SlabName.Equals(slabName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> Insert(SlabMaster entity)
        {
            await _context.SlabMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSlabMasters);
            return result;
        }

        public async Task<int> Update(SlabMaster entity)
        {
            _context.SlabMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSlabMasters);
            return result;
        }

        public async Task<int> Delete(int slabMasterId)
        {
            var entity = await _context.SlabMasters.FindAsync(slabMasterId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSlabMasters);
            return result;
        }
    }
}
