using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class APIUserInitialRequestResponseLogRepository : IAPIUserInitialRequestResponseLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<APIUserInitialRequestResponseLog>> _listCache;
        public APIUserInitialRequestResponseLogRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<APIUserInitialRequestResponseLog>>(); }
        public async Task<List<APIUserInitialRequestResponseLog>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllAPIUserInitialRequestResponseLogs, () => _context.APIUserInitialRequestResponseLogs.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<APIUserInitialRequestResponseLog>();
        public async Task<APIUserInitialRequestResponseLog?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.APIUserInitialRequestResponseLogID == id); }
        public async Task<List<APIUserInitialRequestResponseLog>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<List<APIUserInitialRequestResponseLog>> GetByTransaction(int transactionMasterID) { var list = await GetAll(); return list.Where(x => x.TransactionMasterID == transactionMasterID).OrderBy(x => x.APIUserInitialRequestResponseLogID).ToList(); }
        public async Task<int> Insert(APIUserInitialRequestResponseLog entity) { await _context.APIUserInitialRequestResponseLogs.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAPIUserInitialRequestResponseLogs); return result; }
        public async Task<int> Update(APIUserInitialRequestResponseLog entity) { _context.APIUserInitialRequestResponseLogs.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAPIUserInitialRequestResponseLogs); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.APIUserInitialRequestResponseLogs.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAPIUserInitialRequestResponseLogs); return result; }
    }
}
