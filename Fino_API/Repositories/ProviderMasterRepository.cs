using FintechApp.Processing;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderMasterRepository : IProviderMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderMaster>> _listCache;

        public ProviderMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<ProviderMaster>>();
        }

        public async Task<List<ProviderMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllProviderMasters, () =>
                _context.ProviderMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()
            ) ?? new List<ProviderMaster>();
        }

        public async Task<ProviderMaster?> GetByID(int id)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.ProviderID == id);
        }

        public async Task<ProviderMaster?> GetByName(string name)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.ProviderName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> Insert(ProviderMaster entity)
        {
            await _context.ProviderMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderMasters);
            return result;
        }

        public async Task<int> Update(ProviderMaster entity)
        {
            _context.ProviderMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.ProviderMasters.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllProviderMasters);
            return result;
        }
    }
}
