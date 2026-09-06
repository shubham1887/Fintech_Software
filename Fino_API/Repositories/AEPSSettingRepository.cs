using FintechApp.Processing;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class AEPSSettingRepository : IAEPSSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<AEPSSetting>> _listCache;

        public AEPSSettingRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<AEPSSetting>>();
        }

        public async Task<List<AEPSSetting>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllAEPSSettings, () =>
                _context.AEPSSettings.AsNoTracking().Where(x => !x.IsDeleted).ToList()
            ) ?? new List<AEPSSetting>();
        }

        public async Task<AEPSSetting?> GetByID(int id)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.AEPSSettingID == id);
        }

        public async Task<AEPSSetting?> GetByName(string name)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> Insert(AEPSSetting entity)
        {
            await _context.AEPSSettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllAEPSSettings);
            return result;
        }

        public async Task<int> Update(AEPSSetting entity)
        {
            _context.AEPSSettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllAEPSSettings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.AEPSSettings.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllAEPSSettings);
            return result;
        }
    }
}
