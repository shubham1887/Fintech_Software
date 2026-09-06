using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UrlTypeMasterRepository : IUrlTypeMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UrlTypeMaster>> _listCache;
        public UrlTypeMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UrlTypeMaster>>(); }
        public async Task<List<UrlTypeMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUrlTypeMasters, () => _context.UrlTypeMasters.AsNoTracking().ToList()) ?? new List<UrlTypeMaster>();
        public async Task<UrlTypeMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UrlTypeID == id); }

        public async Task<int> Insert(UrlTypeMaster entity) { await _context.UrlTypeMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUrlTypeMasters); return result; }
        public async Task<int> Update(UrlTypeMaster entity) { _context.UrlTypeMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUrlTypeMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UrlTypeMasters.FindAsync(id); if (entity == null) return 0; _context.UrlTypeMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUrlTypeMasters); return result; }
    }
}
