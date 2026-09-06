using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<Category>> _listCache;
        public CategoryRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<Category>>(); }
        public async Task<List<Category>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCategorys, () => _context.Categorys.AsNoTracking().ToList()) ?? new List<Category>();
        public async Task<Category?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CategoryID == id); }

        public async Task<int> Insert(Category entity) { await _context.Categorys.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCategorys); return result; }
        public async Task<int> Update(Category entity) { _context.Categorys.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCategorys); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.Categorys.FindAsync(id); if (entity == null) return 0; _context.Categorys.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCategorys); return result; }
    }
}
