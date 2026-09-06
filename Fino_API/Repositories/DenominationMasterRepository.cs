using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class DenominationMasterRepository : IDenominationMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<DenominationMaster>> _listCache;

        public DenominationMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<DenominationMaster>>();
        }

        public async Task<List<DenominationMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllDenominationMasters, () =>
                _context.DenominationMasters
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<DenominationMaster>();
        }

        public async Task<DenominationMaster?> GetByID(int denominationMasterId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.DenominationMasterID == denominationMasterId);
        }

        public async Task<int> Insert(DenominationMaster entity)
        {
            await _context.DenominationMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDenominationMasters);
            return result;
        }

        public async Task<int> Update(DenominationMaster entity)
        {
            _context.DenominationMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDenominationMasters);
            return result;
        }

        public async Task<int> Delete(int denominationMasterId)
        {
            var entity = await _context.DenominationMasters.FindAsync(denominationMasterId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDenominationMasters);
            return result;
        }
    }
}
