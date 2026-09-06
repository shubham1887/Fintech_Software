using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class AndroidSectionMasterRepository : IAndroidSectionMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<AndroidSectionMaster>> _listCache;
        public AndroidSectionMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<AndroidSectionMaster>>(); }
        public async Task<List<AndroidSectionMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllAndroidSectionMasters, () => _context.AndroidSectionMasters.AsNoTracking().ToList()) ?? new List<AndroidSectionMaster>();
        public async Task<AndroidSectionMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.AndroidSectionMasterID == id); }

        public async Task<int> Insert(AndroidSectionMaster entity) { await _context.AndroidSectionMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAndroidSectionMasters); return result; }
        public async Task<int> Update(AndroidSectionMaster entity) { _context.AndroidSectionMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAndroidSectionMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.AndroidSectionMasters.FindAsync(id); if (entity == null) return 0; _context.AndroidSectionMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllAndroidSectionMasters); return result; }
    }
}
