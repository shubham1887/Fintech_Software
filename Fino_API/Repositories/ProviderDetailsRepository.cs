using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderDetailsRepository : IProviderDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderDetails>> _listCache;

        public ProviderDetailsRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<ProviderDetails>>();
        }

        public async Task<List<ProviderDetails>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllProviderDetails, () =>
                _context.ProviderDetails
                    .Include(x => x.ProviderMaster)
                    .Include(x => x.ServiceMaster)
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<ProviderDetails>();
        }

        public async Task<List<ProviderDetails>> GetByProviderID(int providerId)
        {
            var all = await GetAll();
            return all.Where(x => x.ProviderID == providerId).ToList();
        }

        public async Task<ProviderDetails?> GetByID(int providerDetailId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.ProviderDetailID == providerDetailId);
        }

        public async Task<int> Insert(ProviderDetails entity)
        {
            await _context.ProviderDetails.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderDetails);
            return result;
        }

        public async Task<int> Update(ProviderDetails entity)
        {
            _context.ProviderDetails.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderDetails);
            return result;
        }

        public async Task<int> Delete(int providerDetailId)
        {
            var entity = await _context.ProviderDetails.FindAsync(providerDetailId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderDetails);
            return result;
        }
    }
}
