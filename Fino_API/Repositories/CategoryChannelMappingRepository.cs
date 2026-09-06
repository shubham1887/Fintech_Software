using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class CategoryChannelMappingRepository : ICategoryChannelMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CategoryChannelMapping>> _listCache;
        public CategoryChannelMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CategoryChannelMapping>>(); }
        public async Task<List<CategoryChannelMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCategoryChannelMappings, () => _context.CategoryChannelMappings.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<CategoryChannelMapping>();
        public async Task<CategoryChannelMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CategoryChannelMappingID == id); }

        public async Task<int> Insert(CategoryChannelMapping entity) { await _context.CategoryChannelMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCategoryChannelMappings); return result; }
        public async Task<int> Update(CategoryChannelMapping entity) { _context.CategoryChannelMappings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCategoryChannelMappings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CategoryChannelMappings.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCategoryChannelMappings); return result; }
    }
}
