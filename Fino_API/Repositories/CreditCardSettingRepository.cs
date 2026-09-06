using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CreditCardSettingRepository : ICreditCardSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CreditCardSetting>> _listCache;
        public CreditCardSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CreditCardSetting>>(); }
        public async Task<List<CreditCardSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCreditCardSettings, () => _context.CreditCardSettings.AsNoTracking().ToList()) ?? new List<CreditCardSetting>();
        public async Task<CreditCardSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CreditCardSettingID == id); }

        public async Task<int> Insert(CreditCardSetting entity) { await _context.CreditCardSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCreditCardSettings); return result; }
        public async Task<int> Update(CreditCardSetting entity) { _context.CreditCardSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCreditCardSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CreditCardSettings.FindAsync(id); if (entity == null) return 0; _context.CreditCardSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCreditCardSettings); return result; }
    }
}
