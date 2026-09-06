using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CashCollectionDetailRepository : ICashCollectionDetailRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CashCollectionDetail>> _listCache;
        public CashCollectionDetailRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CashCollectionDetail>>(); }
        public async Task<List<CashCollectionDetail>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCashCollectionDetails, () => _context.CashCollectionDetails.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<CashCollectionDetail>();
        public async Task<CashCollectionDetail?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CashCollectionDetailID == id); }
        public async Task<List<CashCollectionDetail>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(CashCollectionDetail entity) { await _context.CashCollectionDetails.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionDetails); return result; }
        public async Task<int> Update(CashCollectionDetail entity) { _context.CashCollectionDetails.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionDetails); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CashCollectionDetails.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionDetails); return result; }
    }
}
