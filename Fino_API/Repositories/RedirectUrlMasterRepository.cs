using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class RedirectUrlMasterRepository : IRedirectUrlMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<RedirectUrlMaster>> _listCache;
        public RedirectUrlMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<RedirectUrlMaster>>(); }
        public async Task<List<RedirectUrlMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllRedirectUrlMasters, () => _context.RedirectUrlMasters.AsNoTracking().ToList()) ?? new List<RedirectUrlMaster>();
        public async Task<RedirectUrlMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.RedirectUrlMasterID == id); }

        public async Task<int> Insert(RedirectUrlMaster entity) { await _context.RedirectUrlMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRedirectUrlMasters); return result; }
        public async Task<int> Update(RedirectUrlMaster entity) { _context.RedirectUrlMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRedirectUrlMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.RedirectUrlMasters.FindAsync(id); if (entity == null) return 0; _context.RedirectUrlMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRedirectUrlMasters); return result; }
    }
}
