using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class PrepaidActivationRepository : IPrepaidActivationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PrepaidActivation>> _listCache;
        public PrepaidActivationRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<PrepaidActivation>>(); }
        public async Task<List<PrepaidActivation>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPrepaidActivations, () => _context.PrepaidActivations.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<PrepaidActivation>();
        public async Task<PrepaidActivation?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.PrepaidActivationID == id); }
        public async Task<List<PrepaidActivation>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(PrepaidActivation entity) { await _context.PrepaidActivations.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPrepaidActivations); return result; }
        public async Task<int> Update(PrepaidActivation entity) { _context.PrepaidActivations.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPrepaidActivations); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.PrepaidActivations.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPrepaidActivations); return result; }
    }
}
