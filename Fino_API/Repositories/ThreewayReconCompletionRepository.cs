using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class ThreewayReconCompletionRepository : IThreewayReconCompletionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ThreewayReconCompletion>> _listCache;
        public ThreewayReconCompletionRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ThreewayReconCompletion>>(); }
        public async Task<List<ThreewayReconCompletion>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllThreewayReconCompletions, () => _context.ThreewayReconCompletions.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ThreewayReconCompletion>();
        public async Task<ThreewayReconCompletion?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ThreewayReconCompletionID == id); }

        public async Task<int> Insert(ThreewayReconCompletion entity) { await _context.ThreewayReconCompletions.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllThreewayReconCompletions); return result; }
        public async Task<int> Update(ThreewayReconCompletion entity) { _context.ThreewayReconCompletions.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllThreewayReconCompletions); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ThreewayReconCompletions.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllThreewayReconCompletions); return result; }
    }
}
