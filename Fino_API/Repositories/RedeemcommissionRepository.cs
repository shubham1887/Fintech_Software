using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class RedeemcommissionRepository : IRedeemcommissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<Redeemcommission>> _listCache;
        public RedeemcommissionRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<Redeemcommission>>(); }
        public async Task<List<Redeemcommission>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllRedeemcommissions, () => _context.Redeemcommissions.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<Redeemcommission>();
        public async Task<Redeemcommission?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.RedeemcommissionID == id); }
        public async Task<List<Redeemcommission>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(Redeemcommission entity) { await _context.Redeemcommissions.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRedeemcommissions); return result; }
        public async Task<int> Update(Redeemcommission entity) { _context.Redeemcommissions.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRedeemcommissions); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.Redeemcommissions.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRedeemcommissions); return result; }
    }
}
