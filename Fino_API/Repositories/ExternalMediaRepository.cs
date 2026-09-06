using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ExternalMediaRepository : IExternalMediaRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ExternalMedia>> _listCache;
        public ExternalMediaRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ExternalMedia>>(); }
        public async Task<List<ExternalMedia>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllExternalMedias, () => _context.ExternalMedias.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ExternalMedia>();
        public async Task<ExternalMedia?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ExternalMediaID == id); }

        public async Task<int> Insert(ExternalMedia entity) { await _context.ExternalMedias.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllExternalMedias); return result; }
        public async Task<int> Update(ExternalMedia entity) { _context.ExternalMedias.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllExternalMedias); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ExternalMedias.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllExternalMedias); return result; }
    }
}
