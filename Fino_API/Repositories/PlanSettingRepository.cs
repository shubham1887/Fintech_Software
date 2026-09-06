using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class PlanSettingRepository : IPlanSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PlanSetting>> _listCache;
        public PlanSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<PlanSetting>>(); }
        public async Task<List<PlanSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPlanSettings, () => _context.PlanSettings.AsNoTracking().ToList()) ?? new List<PlanSetting>();
        public async Task<PlanSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.PlanSettingID == id); }

        public async Task<int> Insert(PlanSetting entity) { await _context.PlanSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPlanSettings); return result; }
        public async Task<int> Update(PlanSetting entity) { _context.PlanSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPlanSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.PlanSettings.FindAsync(id); if (entity == null) return 0; _context.PlanSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPlanSettings); return result; }
    }
}
