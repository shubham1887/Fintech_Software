using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderResponseMasterRepository : IProviderResponseMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderResponseMaster>> _listCache;

        public ProviderResponseMasterRepository(ApplicationDbContext context, MemoryCacheProcessing<List<ProviderResponseMaster>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<ProviderResponseMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllProviderResponseMasters, () =>
                _context.ProviderResponseMasters.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.ProviderMaster)
                    .ToList()
            ) ?? new List<ProviderResponseMaster>();
        }

        public async Task<List<ProviderResponseMaster>> GetByProviderID(int providerId)
        {
            var list = await GetAll();
            return list.Where(x => x.ProviderID == providerId).ToList();
        }

        public async Task<ProviderResponseMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.ProviderResponseMasterID == id);
        }

        public async Task<int> Insert(ProviderResponseMaster entity)
        {
            await _context.ProviderResponseMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderResponseMasters);
            return result;
        }

        public async Task<int> Update(ProviderResponseMaster entity)
        {
            _context.ProviderResponseMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderResponseMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.ProviderResponseMasters.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderResponseMasters);
            return result;
        }
    }
}
