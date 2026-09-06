using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderOperatorMasterRepository : IProviderOperatorMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderOperatorMaster>> _listCache;

        public ProviderOperatorMasterRepository(ApplicationDbContext context, MemoryCacheProcessing<List<ProviderOperatorMaster>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<ProviderOperatorMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllProviderOperatorMasters, () =>
                _context.ProviderOperatorMasters.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<ProviderOperatorMaster>();
        }

        public async Task<List<ProviderOperatorMaster>> GetByProviderID(int providerId)
        {
            var list = await GetAll();
            return list.Where(x => x.ProviderID == providerId).ToList();
        }

        public async Task<ProviderOperatorMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.ProviderOperatorID == id);
        }

        public async Task<int> Insert(ProviderOperatorMaster entity)
        {
            await _context.ProviderOperatorMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderOperatorMasters);
            return result;
        }

        public async Task<int> Update(ProviderOperatorMaster entity)
        {
            _context.ProviderOperatorMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderOperatorMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.ProviderOperatorMasters.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderOperatorMasters);
            return result;
        }
    }
}
