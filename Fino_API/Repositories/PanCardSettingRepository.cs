using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class PanCardSettingRepository : IPanCardSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PanCardSetting>> _listCache;
        public PanCardSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<PanCardSetting>>(); }
        public async Task<List<PanCardSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPanCardSettings, () => _context.PanCardSettings.AsNoTracking().ToList()) ?? new List<PanCardSetting>();
        public async Task<PanCardSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.PanCardSettingID == id); }

        public async Task<int> Insert(PanCardSetting entity) { await _context.PanCardSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPanCardSettings); return result; }
        public async Task<int> Update(PanCardSetting entity) { _context.PanCardSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPanCardSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.PanCardSettings.FindAsync(id); if (entity == null) return 0; _context.PanCardSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPanCardSettings); return result; }
    }
}
