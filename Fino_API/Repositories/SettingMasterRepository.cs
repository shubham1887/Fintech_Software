using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class SettingMasterRepository : ISettingMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SettingMaster>> _listCache;

        public SettingMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<SettingMaster>>();
        }

        public async Task<List<SettingMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllSettingMasters, () =>
                _context.SettingMasters
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<SettingMaster>();
        }

        public async Task<SettingMaster?> GetByID(int settingMasterId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.SettingMasterID == settingMasterId);
        }

        public async Task<SettingMaster?> GetByKey(string key)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> Insert(SettingMaster entity)
        {
            await _context.SettingMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSettingMasters);
            return result;
        }

        public async Task<int> Update(SettingMaster entity)
        {
            _context.SettingMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSettingMasters);
            return result;
        }

        public async Task<int> Delete(int settingMasterId)
        {
            var entity = await _context.SettingMasters.FindAsync(settingMasterId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSettingMasters);
            return result;
        }
    }
}
