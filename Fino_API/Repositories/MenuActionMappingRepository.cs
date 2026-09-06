using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class MenuActionMappingRepository : IMenuActionMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<MenuActionMapping>> _listCache;
        public MenuActionMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<MenuActionMapping>>(); }
        public async Task<List<MenuActionMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllMenuActionMappings, () => _context.MenuActionMappings.AsNoTracking().ToList()) ?? new List<MenuActionMapping>();
        public async Task<MenuActionMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.MenuActionMappingID == id); }

        public async Task<int> Insert(MenuActionMapping entity) { await _context.MenuActionMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMenuActionMappings); return result; }
        public async Task<int> Update(MenuActionMapping entity) { _context.MenuActionMappings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMenuActionMappings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.MenuActionMappings.FindAsync(id); if (entity == null) return 0; _context.MenuActionMappings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMenuActionMappings); return result; }
    }
}
