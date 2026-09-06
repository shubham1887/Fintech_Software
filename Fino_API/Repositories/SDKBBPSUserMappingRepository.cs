using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class SDKBBPSUserMappingRepository : ISDKBBPSUserMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SDKBBPSUserMapping>> _listCache;
        public SDKBBPSUserMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<SDKBBPSUserMapping>>(); }
        public async Task<List<SDKBBPSUserMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSDKBBPSUserMappings, () => _context.SDKBBPSUserMappings.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<SDKBBPSUserMapping>();
        public async Task<SDKBBPSUserMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SDKBBPSUserMappingID == id); }
        public async Task<List<SDKBBPSUserMapping>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(SDKBBPSUserMapping entity) { await _context.SDKBBPSUserMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSDKBBPSUserMappings); return result; }
        public async Task<int> Update(SDKBBPSUserMapping entity) { _context.SDKBBPSUserMappings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSDKBBPSUserMappings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.SDKBBPSUserMappings.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSDKBBPSUserMappings); return result; }
    }
}
