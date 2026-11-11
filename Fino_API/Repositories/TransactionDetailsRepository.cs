using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class TransactionDetailsRepository : ITransactionDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<TransactionDetails>> _listCache;
        public TransactionDetailsRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<TransactionDetails>>(); }
        public async Task<List<TransactionDetails>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllTransactionDetailss, () => _context.TransactionDetailss.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<TransactionDetails>();
        public async Task<List<TransactionDetails>> GetFromDate(DateTime from)
            => await _context.TransactionDetailss.AsNoTracking()
                .Where(x => !x.IsDeleted && x.DateTime >= from.Date)
                .ToListAsync();
        public async Task<List<TransactionDetails>> GetByReference(int referenceID)
            => await _context.TransactionDetailss.AsNoTracking()
                .Where(x => !x.IsDeleted && x.ReferenceID == referenceID)
                .ToListAsync();
        public async Task<TransactionDetails?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.TransactionDetailID == id); }
        public async Task<List<TransactionDetails>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(TransactionDetails entity) { await _context.TransactionDetailss.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionDetailss); return result; }
        public async Task<int> Update(TransactionDetails entity) { _context.TransactionDetailss.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionDetailss); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.TransactionDetailss.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionDetailss); return result; }
    }
}
