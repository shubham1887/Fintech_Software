using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class SlabDetailRepository : ISlabDetailRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SlabDetail>> _listCache;

        public SlabDetailRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<SlabDetail>>();
        }

        public async Task<List<SlabDetail>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllSlabDetails, () =>
                _context.SlabDetails
                    .Include(x => x.SlabMaster)
                    .Include(x => x.ServiceMaster)
                    .Include(x => x.OperatorMaster)
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<SlabDetail>();
        }

        public async Task<List<SlabDetail>> GetBySlabMasterID(int slabMasterId)
        {
            var all = await GetAll();
            return all.Where(x => x.SlabMasterID == slabMasterId).ToList();
        }

        public async Task<SlabDetail?> GetByID(int slabDetailId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.SlabDetailID == slabDetailId);
        }

        public async Task<int> Insert(SlabDetail entity)
        {
            await _context.SlabDetails.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSlabDetails);
            return result;
        }

        // GetByID hands back a cached AsNoTracking row whose ServiceMaster/OperatorMaster
        // navigations are loaded, and attaching that graph makes EF resolve the FK from the stale
        // navigation - silently reverting a ServiceID/OperatorID change. Copy the scalars onto a
        // freshly tracked row instead.
        public async Task<int> Update(SlabDetail entity)
        {
            var tracked = await _context.SlabDetails.FirstOrDefaultAsync(x => x.SlabDetailID == entity.SlabDetailID);
            if (tracked == null) return 0;

            _context.Entry(tracked).CurrentValues.SetValues(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSlabDetails);
            return result;
        }

        public async Task<int> Delete(int slabDetailId)
        {
            var entity = await _context.SlabDetails.FindAsync(slabDetailId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSlabDetails);
            return result;
        }
    }
}
