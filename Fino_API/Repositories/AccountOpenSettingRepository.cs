using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class AccountOpenSettingRepository : IAccountOpenSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<AccountOpenSetting>> _listCache;
        public AccountOpenSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<AccountOpenSetting>>(); }
        public async Task<List<AccountOpenSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllAccountOpenSettings, () => _context.AccountOpenSettings.AsNoTracking().ToList()) ?? new List<AccountOpenSetting>();
        public async Task<AccountOpenSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.AccountOpenSettingID == id); }
        public async Task<int> Insert(AccountOpenSetting entity) { await _context.AccountOpenSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAccountOpenSettings); return result; }
        public async Task<int> Update(AccountOpenSetting entity) { _context.AccountOpenSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAccountOpenSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.AccountOpenSettings.FindAsync(id); if (entity == null) return 0; _context.AccountOpenSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAccountOpenSettings); return result; }
    }
}
