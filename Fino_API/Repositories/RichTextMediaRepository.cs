using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class RichTextMediaRepository : IRichTextMediaRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<RichTextMedia>> _listCache;
        public RichTextMediaRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<RichTextMedia>>(); }
        public async Task<List<RichTextMedia>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllRichTextMedias, () => _context.RichTextMedias.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<RichTextMedia>();
        public async Task<RichTextMedia?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ID == id); }

        public async Task<int> Insert(RichTextMedia entity) { await _context.RichTextMedias.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRichTextMedias); return result; }
        public async Task<int> Update(RichTextMedia entity) { _context.RichTextMedias.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRichTextMedias); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.RichTextMedias.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRichTextMedias); return result; }
    }
}
