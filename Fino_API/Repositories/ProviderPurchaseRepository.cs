using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderPurchaseRepository : IProviderPurchaseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderPurchase>> _listCache;
        public ProviderPurchaseRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ProviderPurchase>>(); }
        public async Task<List<ProviderPurchase>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllProviderPurchases, () => _context.ProviderPurchases.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ProviderPurchase>();
        public async Task<ProviderPurchase?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ProviderPurchaseID == id); }
        public async Task<List<ProviderPurchase>> GetByProviderID(int providerid) { var list = await GetAll(); return list.Where(x => x.ProviderID == providerid).ToList(); }
        public async Task<int> Insert(ProviderPurchase entity) { await _context.ProviderPurchases.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderPurchases); return result; }
        public async Task<int> Update(ProviderPurchase entity) { _context.ProviderPurchases.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderPurchases); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ProviderPurchases.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderPurchases); return result; }
    }
}
