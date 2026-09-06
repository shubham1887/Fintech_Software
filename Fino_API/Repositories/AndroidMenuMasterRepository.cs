using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class AndroidMenuMasterRepository : IAndroidMenuMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<AndroidMenuMaster>> _listCache;
        public AndroidMenuMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<AndroidMenuMaster>>(); }
        public async Task<List<AndroidMenuMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllAndroidMenuMasters, () => _context.AndroidMenuMasters.AsNoTracking().ToList()) ?? new List<AndroidMenuMaster>();
        public async Task<AndroidMenuMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.AndroidMenuMasterID == id); }

        public async Task<int> Insert(AndroidMenuMaster entity) { await _context.AndroidMenuMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAndroidMenuMasters); return result; }
        public async Task<int> Update(AndroidMenuMaster entity) { _context.AndroidMenuMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAndroidMenuMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.AndroidMenuMasters.FindAsync(id); if (entity == null) return 0; _context.AndroidMenuMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAndroidMenuMasters); return result; }
    }
}
