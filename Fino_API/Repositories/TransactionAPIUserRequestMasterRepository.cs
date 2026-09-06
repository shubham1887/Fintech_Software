using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class TransactionAPIUserRequestMasterRepository : ITransactionAPIUserRequestMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<TransactionAPIUserRequestMaster>> _listCache;
        public TransactionAPIUserRequestMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<TransactionAPIUserRequestMaster>>(); }
        public async Task<List<TransactionAPIUserRequestMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllTransactionAPIUserRequestMasters, () => _context.TransactionAPIUserRequestMasters.AsNoTracking().ToList()) ?? new List<TransactionAPIUserRequestMaster>();
        public async Task<TransactionAPIUserRequestMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.TransactionAPIUserRequestMasterID == id); }
        public async Task<List<TransactionAPIUserRequestMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(TransactionAPIUserRequestMaster entity) { await _context.TransactionAPIUserRequestMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionAPIUserRequestMasters); return result; }
        public async Task<int> Update(TransactionAPIUserRequestMaster entity) { _context.TransactionAPIUserRequestMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionAPIUserRequestMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.TransactionAPIUserRequestMasters.FindAsync(id); if (entity == null) return 0; _context.TransactionAPIUserRequestMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTransactionAPIUserRequestMasters); return result; }
    }
}
