using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class LeadGenerationSettingRepository : ILeadGenerationSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<LeadGenerationSetting>> _listCache;
        public LeadGenerationSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<LeadGenerationSetting>>(); }
        public async Task<List<LeadGenerationSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllLeadGenerationSettings, () => _context.LeadGenerationSettings.AsNoTracking().ToList()) ?? new List<LeadGenerationSetting>();
        public async Task<LeadGenerationSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.LeadGenerationSettingID == id); }

        public async Task<int> Insert(LeadGenerationSetting entity) { await _context.LeadGenerationSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLeadGenerationSettings); return result; }
        public async Task<int> Update(LeadGenerationSetting entity) { _context.LeadGenerationSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLeadGenerationSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.LeadGenerationSettings.FindAsync(id); if (entity == null) return 0; _context.LeadGenerationSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLeadGenerationSettings); return result; }
    }
}
