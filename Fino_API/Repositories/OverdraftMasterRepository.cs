using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class OverdraftMasterRepository : IOverdraftMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OverdraftMaster>> _listCache;
        public OverdraftMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<OverdraftMaster>>(); }
        public async Task<List<OverdraftMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllOverdraftMasters, () => _context.OverdraftMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<OverdraftMaster>();
        public async Task<OverdraftMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.OverdraftMasterID == id); }
        public async Task<List<OverdraftMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(OverdraftMaster entity) { await _context.OverdraftMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOverdraftMasters); return result; }
        public async Task<int> Update(OverdraftMaster entity) { _context.OverdraftMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOverdraftMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.OverdraftMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOverdraftMasters); return result; }
    }
}
