using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CashDepositSettingRepository : ICashDepositSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CashDepositSetting>> _listCache;
        public CashDepositSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CashDepositSetting>>(); }
        public async Task<List<CashDepositSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCashDepositSettings, () => _context.CashDepositSettings.AsNoTracking().ToList()) ?? new List<CashDepositSetting>();
        public async Task<CashDepositSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CashDepositSettingID == id); }
        public async Task<int> Insert(CashDepositSetting entity) { await _context.CashDepositSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashDepositSettings); return result; }
        public async Task<int> Update(CashDepositSetting entity) { _context.CashDepositSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashDepositSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CashDepositSettings.FindAsync(id); if (entity == null) return 0; _context.CashDepositSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashDepositSettings); return result; }
    }
}
