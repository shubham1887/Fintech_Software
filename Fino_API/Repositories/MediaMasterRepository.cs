using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class MediaMasterRepository : IMediaMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<MediaMaster>> _listCache;
        public MediaMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<MediaMaster>>(); }
        public async Task<List<MediaMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllMediaMasters, () => _context.MediaMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<MediaMaster>();
        public async Task<MediaMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.MediaMasterID == id); }
        public async Task<List<MediaMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(MediaMaster entity) { await _context.MediaMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMediaMasters); return result; }
        public async Task<int> Update(MediaMaster entity) { _context.MediaMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMediaMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.MediaMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllMediaMasters); return result; }
    }
}
