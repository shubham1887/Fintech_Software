using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class TransactionManualUpdateRepository : ITransactionManualUpdateRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<TransactionManualUpdate>> _listCache;
        public TransactionManualUpdateRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<TransactionManualUpdate>>(); }
        public async Task<List<TransactionManualUpdate>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllTransactionManualUpdates, () => _context.TransactionManualUpdates.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<TransactionManualUpdate>();
        public async Task<TransactionManualUpdate?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.TransactionManualUpdateID == id); }

        public async Task<int> Insert(TransactionManualUpdate entity) { await _context.TransactionManualUpdates.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionManualUpdates); return result; }
        public async Task<int> Update(TransactionManualUpdate entity) { _context.TransactionManualUpdates.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionManualUpdates); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.TransactionManualUpdates.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionManualUpdates); return result; }
    }
}
