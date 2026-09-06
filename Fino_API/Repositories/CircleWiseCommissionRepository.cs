using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CircleWiseCommissionRepository : ICircleWiseCommissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CircleWiseCommission>> _listCache;

        public CircleWiseCommissionRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<CircleWiseCommission>>();
        }

        public async Task<List<CircleWiseCommission>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllCircleWiseCommissions, () =>
                _context.CircleWiseCommissions.AsNoTracking()
                    .Include(x => x.SlabMaster)
                    .Include(x => x.OperatorMaster)
                    .ToList()
            ) ?? new List<CircleWiseCommission>();
        }

        public async Task<CircleWiseCommission?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.CircleWiseCommissionID == id);
        }

        public async Task<List<CircleWiseCommission>> GetBySlabMasterID(int slabMasterID)
        {
            var list = await GetAll();
            return list.Where(x => x.SlabMasterID == slabMasterID).ToList();
        }

        public async Task<int> Insert(CircleWiseCommission entity)
        {
            await _context.CircleWiseCommissions.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllCircleWiseCommissions);
            return result;
        }

        public async Task<int> Update(CircleWiseCommission entity)
        {
            _context.CircleWiseCommissions.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllCircleWiseCommissions);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.CircleWiseCommissions.FindAsync(id);
            if (entity == null) return 0;
            _context.CircleWiseCommissions.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllCircleWiseCommissions);
            return result;
        }
    }
}
