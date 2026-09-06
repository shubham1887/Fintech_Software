using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.Requests.UISetting;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UISettingRepository : IUISettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UISettingMaster>> _listCache;

        public UISettingRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<UISettingMaster>>();
        }

        public async Task<List<UISettingMaster>> GetAllUISettings()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUISettings, () =>
            {
                return _context.UISettingMasters
                    .Where(s => !s.IsDeleted)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<UISettingMaster>();
        }

        public async Task<UISettingMaster?> GetUISettingById(int uiSettingMasterID)
        {
            var all = await GetAllUISettings();
            return all.FirstOrDefault(s => s.UISettingMasterID == uiSettingMasterID);
        }

        public async Task<UISettingMaster?> GetUISettingByKey(string key)
        {
            var all = await GetAllUISettings();
            return all.FirstOrDefault(s => s.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<UISettingMaster>> GetUISettings(GetUISettingRequest request)
        {
            var all = await GetAllUISettings();
            var query = all.AsQueryable();

            if (request.UISettingMasterID > 0)
                query = query.Where(s => s.UISettingMasterID == request.UISettingMasterID);

            if (!string.IsNullOrEmpty(request.Key))
                query = query.Where(s => s.Key.Contains(request.Key, StringComparison.OrdinalIgnoreCase));

            var skip = (request.PageNumber - 1) * request.PageSize;
            return query.Skip(skip).Take(request.PageSize).ToList();
        }

        public async Task<int> Insert(UISettingMaster uiSetting)
        {
            await _context.UISettingMasters.AddAsync(uiSetting);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearUISettingCache();

            return result;
        }

        public async Task<int> Update(UISettingMaster uiSetting)
        {
            _context.UISettingMasters.Update(uiSetting);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearUISettingCache();

            return result;
        }

        public async Task<int> Delete(UISettingMaster uiSetting)
        {
            _context.UISettingMasters.Update(uiSetting);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
                ClearUISettingCache();

            return result;
        }

        private void ClearUISettingCache()
        {
            _listCache.Remove(CacheKeys.AllUISettings);
        }
    }
}