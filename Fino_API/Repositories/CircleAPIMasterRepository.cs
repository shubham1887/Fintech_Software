using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CircleAPIMasterRepository : ICircleAPIMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CircleAPIMaster>> _listCache;
        public CircleAPIMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CircleAPIMaster>>(); }
        public async Task<List<CircleAPIMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCircleAPIMasters, () => _context.CircleAPIMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<CircleAPIMaster>();
        public async Task<CircleAPIMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CircleAPIMasterID == id); }

        public async Task<int> Insert(CircleAPIMaster entity) { await _context.CircleAPIMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCircleAPIMasters); return result; }
        public async Task<int> Update(CircleAPIMaster entity) { _context.CircleAPIMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCircleAPIMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CircleAPIMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCircleAPIMasters); return result; }
    }
}
