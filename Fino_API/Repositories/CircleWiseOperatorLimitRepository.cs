using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CircleWiseOperatorLimitRepository : ICircleWiseOperatorLimitRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CircleWiseOperatorLimit>> _listCache;
        public CircleWiseOperatorLimitRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CircleWiseOperatorLimit>>(); }
        public async Task<List<CircleWiseOperatorLimit>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCircleWiseOperatorLimits, () => _context.CircleWiseOperatorLimits.AsNoTracking().ToList()) ?? new List<CircleWiseOperatorLimit>();
        public async Task<CircleWiseOperatorLimit?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CircleWiseOperatorLimitID == id); }
        public async Task<List<CircleWiseOperatorLimit>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(CircleWiseOperatorLimit entity) { await _context.CircleWiseOperatorLimits.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCircleWiseOperatorLimits); return result; }
        public async Task<int> Update(CircleWiseOperatorLimit entity) { _context.CircleWiseOperatorLimits.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCircleWiseOperatorLimits); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CircleWiseOperatorLimits.FindAsync(id); if (entity == null) return 0; _context.CircleWiseOperatorLimits.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCircleWiseOperatorLimits); return result; }
    }
}
