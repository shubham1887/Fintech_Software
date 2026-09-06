using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class RangeWiseSequenceRepository : IRangeWiseSequenceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<RangeWiseSequence>> _listCache;

        public RangeWiseSequenceRepository(ApplicationDbContext context, MemoryCacheProcessing<List<RangeWiseSequence>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<RangeWiseSequence>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllRangeWiseSequences, () =>
                _context.RangeWiseSequences.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.ProviderMaster)
                    .Include(x => x.ServiceMaster)
                    .Include(x => x.OperatorMaster)
                    .ToList()
            ) ?? new List<RangeWiseSequence>();
        }

        public async Task<List<RangeWiseSequence>> GetByOperatorID(int operatorId)
        {
            var list = await GetAll();
            return list.Where(x => x.OperatorID == operatorId).ToList();
        }

        public async Task<RangeWiseSequence?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.RangeWiseSequenceID == id);
        }

        public async Task<int> Insert(RangeWiseSequence entity)
        {
            await _context.RangeWiseSequences.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllRangeWiseSequences);
            return result;
        }

        public async Task<int> Update(RangeWiseSequence entity)
        {
            _context.RangeWiseSequences.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllRangeWiseSequences);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.RangeWiseSequences.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllRangeWiseSequences);
            return result;
        }
    }
}
