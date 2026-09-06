using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class MenuMasterRepository : IMenuMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<MenuMaster>> _listCache;
        public MenuMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<MenuMaster>>(); }
        public async Task<List<MenuMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllMenuMasters, () => _context.MenuMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<MenuMaster>();
        public async Task<MenuMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.MenuMasterID == id); }

        public async Task<int> Insert(MenuMaster entity) { await _context.MenuMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMenuMasters); return result; }
        public async Task<int> Update(MenuMaster entity) { _context.MenuMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMenuMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.MenuMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMenuMasters); return result; }
    }
}
