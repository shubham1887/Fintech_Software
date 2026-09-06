using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class BrowserUserAgentMasterRepository : IBrowserUserAgentMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<BrowserUserAgentMaster>> _listCache;
        public BrowserUserAgentMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<BrowserUserAgentMaster>>(); }
        public async Task<List<BrowserUserAgentMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllBrowserUserAgentMasters, () => _context.BrowserUserAgentMasters.AsNoTracking().ToList()) ?? new List<BrowserUserAgentMaster>();
        public async Task<BrowserUserAgentMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.BrowserUserAgentMasterID == id); }

        public async Task<int> Insert(BrowserUserAgentMaster entity) { await _context.BrowserUserAgentMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBrowserUserAgentMasters); return result; }
        public async Task<int> Update(BrowserUserAgentMaster entity) { _context.BrowserUserAgentMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBrowserUserAgentMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.BrowserUserAgentMasters.FindAsync(id); if (entity == null) return 0; _context.BrowserUserAgentMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBrowserUserAgentMasters); return result; }
    }
}
