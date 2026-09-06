using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserwiseSaleTargetRepository : IUserwiseSaleTargetRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserwiseSaleTarget>> _listCache;
        public UserwiseSaleTargetRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserwiseSaleTarget>>(); }
        public async Task<List<UserwiseSaleTarget>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserwiseSaleTargets, () => _context.UserwiseSaleTargets.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserwiseSaleTarget>();
        public async Task<UserwiseSaleTarget?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserwiseSaleTargetID == id); }
        public async Task<List<UserwiseSaleTarget>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserwiseSaleTarget entity) { await _context.UserwiseSaleTargets.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseSaleTargets); return result; }
        public async Task<int> Update(UserwiseSaleTarget entity) { _context.UserwiseSaleTargets.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseSaleTargets); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserwiseSaleTargets.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseSaleTargets); return result; }
    }
}
