using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class TransactionProviderHistoryRepository : ITransactionProviderHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<TransactionProviderHistory>> _listCache;
        public TransactionProviderHistoryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<TransactionProviderHistory>>(); }
        public async Task<List<TransactionProviderHistory>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllTransactionProviderHistorys, () => _context.TransactionProviderHistorys.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<TransactionProviderHistory>();
        public async Task<TransactionProviderHistory?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.RechargeProviderHistoryID == id); }

        public async Task<int> Insert(TransactionProviderHistory entity) { await _context.TransactionProviderHistorys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionProviderHistorys); return result; }
        public async Task<int> Update(TransactionProviderHistory entity) { _context.TransactionProviderHistorys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionProviderHistorys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.TransactionProviderHistorys.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionProviderHistorys); return result; }
    }
}
