using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class TargetSaleCommissionRepository : ITargetSaleCommissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<TargetSaleCommission>> _listCache;
        public TargetSaleCommissionRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<TargetSaleCommission>>(); }
        public async Task<List<TargetSaleCommission>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllTargetSaleCommissions, () => _context.TargetSaleCommissions.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<TargetSaleCommission>();
        public async Task<TargetSaleCommission?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.TargetSaleCommissionID == id); }
        public async Task<List<TargetSaleCommission>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(TargetSaleCommission entity) { await _context.TargetSaleCommissions.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTargetSaleCommissions); return result; }
        public async Task<int> Update(TargetSaleCommission entity) { _context.TargetSaleCommissions.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTargetSaleCommissions); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.TargetSaleCommissions.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllTargetSaleCommissions); return result; }
    }
}
