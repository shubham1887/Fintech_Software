using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class SmsTransactionMasterRepository : ISmsTransactionMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SmsTransactionMaster>> _listCache;
        public SmsTransactionMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<SmsTransactionMaster>>(); }
        public async Task<List<SmsTransactionMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSmsTransactionMasters, () => _context.SmsTransactionMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<SmsTransactionMaster>();
        public async Task<SmsTransactionMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SmsTransactionMasterID == id); }

        public async Task<int> Insert(SmsTransactionMaster entity) { await _context.SmsTransactionMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSmsTransactionMasters); return result; }
        public async Task<int> Update(SmsTransactionMaster entity) { _context.SmsTransactionMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSmsTransactionMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.SmsTransactionMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSmsTransactionMasters); return result; }
    }
}
