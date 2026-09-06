using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ROfferCommissionRepository : IROfferCommissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ROfferCommission>> _listCache;
        public ROfferCommissionRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ROfferCommission>>(); }
        public async Task<List<ROfferCommission>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllROfferCommissions, () => _context.ROfferCommissions.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ROfferCommission>();
        public async Task<ROfferCommission?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ROfferCommissionID == id); }
        public async Task<List<ROfferCommission>> GetByOperatorID(int operatorid) { var list = await GetAll(); return list.Where(x => x.OperatorID == operatorid).ToList(); }
        public async Task<int> Insert(ROfferCommission entity) { await _context.ROfferCommissions.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllROfferCommissions); return result; }
        public async Task<int> Update(ROfferCommission entity) { _context.ROfferCommissions.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllROfferCommissions); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ROfferCommissions.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllROfferCommissions); return result; }
    }
}
