using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class ChannelCategoryRepository : IChannelCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ChannelCategory>> _listCache;
        public ChannelCategoryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ChannelCategory>>(); }
        public async Task<List<ChannelCategory>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllChannelCategorys, () => _context.ChannelCategorys.AsNoTracking().ToList()) ?? new List<ChannelCategory>();
        public async Task<ChannelCategory?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ChannelCategoryID == id); }

        public async Task<int> Insert(ChannelCategory entity) { await _context.ChannelCategorys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllChannelCategorys); return result; }
        public async Task<int> Update(ChannelCategory entity) { _context.ChannelCategorys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllChannelCategorys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ChannelCategorys.FindAsync(id); if (entity == null) return 0; _context.ChannelCategorys.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllChannelCategorys); return result; }
    }
}
