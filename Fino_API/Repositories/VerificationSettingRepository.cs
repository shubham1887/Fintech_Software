using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class VerificationSettingRepository : IVerificationSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<VerificationSetting>> _listCache;
        public VerificationSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<VerificationSetting>>(); }
        public async Task<List<VerificationSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllVerificationSettings, () => _context.VerificationSettings.AsNoTracking().ToList()) ?? new List<VerificationSetting>();
        public async Task<VerificationSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.VerificationSettingID == id); }

        public async Task<int> Insert(VerificationSetting entity) { await _context.VerificationSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllVerificationSettings); return result; }
        public async Task<int> Update(VerificationSetting entity) { _context.VerificationSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllVerificationSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.VerificationSettings.FindAsync(id); if (entity == null) return 0; _context.VerificationSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllVerificationSettings); return result; }
    }
}
