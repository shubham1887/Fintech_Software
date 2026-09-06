using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class SupportUserAccessDetailRepository : ISupportUserAccessDetailRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SupportUserAccessDetail>> _listCache;
        public SupportUserAccessDetailRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<SupportUserAccessDetail>>(); }
        public async Task<List<SupportUserAccessDetail>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSupportUserAccessDetails, () => _context.SupportUserAccessDetails.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<SupportUserAccessDetail>();
        public async Task<SupportUserAccessDetail?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SupportUserAccessDetailID == id); }
        public async Task<List<SupportUserAccessDetail>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(SupportUserAccessDetail entity) { await _context.SupportUserAccessDetails.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSupportUserAccessDetails); return result; }
        public async Task<int> Update(SupportUserAccessDetail entity) { _context.SupportUserAccessDetails.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSupportUserAccessDetails); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.SupportUserAccessDetails.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSupportUserAccessDetails); return result; }
    }
}
