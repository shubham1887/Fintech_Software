using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class SubscriptionMasterRepository : ISubscriptionMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SubscriptionMaster>> _listCache;
        public SubscriptionMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<SubscriptionMaster>>(); }
        public async Task<List<SubscriptionMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSubscriptionMasters, () => _context.SubscriptionMasters.AsNoTracking().ToList()) ?? new List<SubscriptionMaster>();
        public async Task<SubscriptionMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SubscriptionMasterID == id); }

        public async Task<int> Insert(SubscriptionMaster entity) { await _context.SubscriptionMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSubscriptionMasters); return result; }
        public async Task<int> Update(SubscriptionMaster entity) { _context.SubscriptionMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSubscriptionMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.SubscriptionMasters.FindAsync(id); if (entity == null) return 0; _context.SubscriptionMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSubscriptionMasters); return result; }
    }
}
