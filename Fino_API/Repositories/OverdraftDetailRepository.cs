using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class OverdraftDetailRepository : IOverdraftDetailRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OverdraftDetail>> _listCache;
        public OverdraftDetailRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<OverdraftDetail>>(); }
        public async Task<List<OverdraftDetail>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllOverdraftDetails, () => _context.OverdraftDetails.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<OverdraftDetail>();
        public async Task<OverdraftDetail?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.OverdraftDetailID == id); }
        public async Task<List<OverdraftDetail>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(OverdraftDetail entity) { await _context.OverdraftDetails.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOverdraftDetails); return result; }
        public async Task<int> Update(OverdraftDetail entity) { _context.OverdraftDetails.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOverdraftDetails); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.OverdraftDetails.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOverdraftDetails); return result; }
    }
}
