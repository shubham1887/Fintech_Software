using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;

namespace FintechApp.Repositories
{
    public class DMTSettingRepository : IDMTSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<DMTSetting>> _listCache;

        public DMTSettingRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<DMTSetting>>();
        }

        public async Task<List<DMTSetting>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllDMTSettings, () =>
                _context.DMTSettings.ToList()
            ) ?? new List<DMTSetting>();
        }

        public async Task<DMTSetting?> GetByID(int dmtSettingId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.DMTSettingID == dmtSettingId);
        }

        public async Task<int> Insert(DMTSetting entity)
        {
            await _context.DMTSettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDMTSettings);
            return result;
        }

        public async Task<int> Update(DMTSetting entity)
        {
            _context.DMTSettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDMTSettings);
            return result;
        }

        public async Task<int> Delete(int dmtSettingId)
        {
            var entity = await _context.DMTSettings.FindAsync(dmtSettingId);
            if (entity == null) return 0;
            _context.DMTSettings.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDMTSettings);
            return result;
        }
    }
}
