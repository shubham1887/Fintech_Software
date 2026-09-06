using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderCallBackLogRepository : IProviderCallBackLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderCallBackLog>> _listCache;
        public ProviderCallBackLogRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ProviderCallBackLog>>(); }
        public async Task<List<ProviderCallBackLog>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllProviderCallBackLogs, () => _context.ProviderCallBackLogs.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ProviderCallBackLog>();
        public async Task<List<ProviderCallBackLog>> GetByTransaction(int transactionMasterId) { var list = await GetAll(); return list.Where(x => x.TransactionMasterID == transactionMasterId).OrderBy(x => x.ProviderCallBackID).ToList(); }
        public async Task<ProviderCallBackLog?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ProviderCallBackID == id); }

        public async Task<int> Insert(ProviderCallBackLog entity) { await _context.ProviderCallBackLogs.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderCallBackLogs); return result; }
        public async Task<int> Update(ProviderCallBackLog entity) { _context.ProviderCallBackLogs.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderCallBackLogs); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ProviderCallBackLogs.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderCallBackLogs); return result; }
    }
}
