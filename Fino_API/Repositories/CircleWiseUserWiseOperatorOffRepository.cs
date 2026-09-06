using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CircleWiseUserWiseOperatorOffRepository : ICircleWiseUserWiseOperatorOffRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CircleWiseUserWiseOperatorOff>> _listCache;
        public CircleWiseUserWiseOperatorOffRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CircleWiseUserWiseOperatorOff>>(); }
        public async Task<List<CircleWiseUserWiseOperatorOff>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCircleWiseUserWiseOperatorOffs, () => _context.CircleWiseUserWiseOperatorOffs.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<CircleWiseUserWiseOperatorOff>();
        public async Task<CircleWiseUserWiseOperatorOff?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CircleWiseUserWiseOperatorOffID == id); }
        public async Task<List<CircleWiseUserWiseOperatorOff>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(CircleWiseUserWiseOperatorOff entity) { await _context.CircleWiseUserWiseOperatorOffs.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCircleWiseUserWiseOperatorOffs); return result; }
        public async Task<int> Update(CircleWiseUserWiseOperatorOff entity) { _context.CircleWiseUserWiseOperatorOffs.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCircleWiseUserWiseOperatorOffs); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CircleWiseUserWiseOperatorOffs.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCircleWiseUserWiseOperatorOffs); return result; }
    }
}
