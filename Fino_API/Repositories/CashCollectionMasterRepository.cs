using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CashCollectionMasterRepository : ICashCollectionMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CashCollectionMaster>> _listCache;
        public CashCollectionMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CashCollectionMaster>>(); }
        public async Task<List<CashCollectionMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCashCollectionMasters, () => _context.CashCollectionMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<CashCollectionMaster>();
        public async Task<CashCollectionMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CashCollectionMasterID == id); }
        public async Task<List<CashCollectionMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(CashCollectionMaster entity) { await _context.CashCollectionMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionMasters); return result; }
        public async Task<int> Update(CashCollectionMaster entity) { _context.CashCollectionMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CashCollectionMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionMasters); return result; }
    }
}
