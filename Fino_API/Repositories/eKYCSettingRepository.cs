using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class eKYCSettingRepository : IeKYCSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<eKYCSetting>> _listCache;
        public eKYCSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<eKYCSetting>>(); }
        public async Task<List<eKYCSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AlleKYCSettings, () => _context.eKYCSettings.AsNoTracking().ToList()) ?? new List<eKYCSetting>();
        public async Task<eKYCSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.eKYCSettingID == id); }

        public async Task<int> Insert(eKYCSetting entity) { await _context.eKYCSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AlleKYCSettings); return result; }
        public async Task<int> Update(eKYCSetting entity) { _context.eKYCSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AlleKYCSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.eKYCSettings.FindAsync(id); if (entity == null) return 0; _context.eKYCSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AlleKYCSettings); return result; }
    }
}
