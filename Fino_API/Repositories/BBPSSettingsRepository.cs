using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class BBPSSettingsRepository : IBBPSSettingsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<BBPSSettings>> _listCache;

        public BBPSSettingsRepository(ApplicationDbContext context, MemoryCacheProcessing<List<BBPSSettings>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<BBPSSettings>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllBBPSSettings, () =>
                _context.BBPSSettings.AsNoTracking().ToList()
            ) ?? new List<BBPSSettings>();
        }

        public async Task<BBPSSettings?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.BBPSSettingID == id);
        }

        public async Task<int> Insert(BBPSSettings entity)
        {
            await _context.BBPSSettings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllBBPSSettings);
            return result;
        }

        public async Task<int> Update(BBPSSettings entity)
        {
            _context.BBPSSettings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllBBPSSettings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.BBPSSettings.FindAsync(id);
            if (entity == null) return 0;
            _context.BBPSSettings.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllBBPSSettings);
            return result;
        }
    }
}
