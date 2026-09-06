using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class SubCategoryRepository : ISubCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SubCategory>> _listCache;
        public SubCategoryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<SubCategory>>(); }
        public async Task<List<SubCategory>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSubCategorys, () => _context.SubCategorys.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<SubCategory>();
        public async Task<SubCategory?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SubCategoryID == id); }

        public async Task<int> Insert(SubCategory entity) { await _context.SubCategorys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSubCategorys); return result; }
        public async Task<int> Update(SubCategory entity) { _context.SubCategorys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSubCategorys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.SubCategorys.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSubCategorys); return result; }
    }
}
