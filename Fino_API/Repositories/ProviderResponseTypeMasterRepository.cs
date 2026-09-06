using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderResponseTypeMasterRepository : IProviderResponseTypeMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderResponseTypeMaster>> _listCache;
        public ProviderResponseTypeMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ProviderResponseTypeMaster>>(); }
        public async Task<List<ProviderResponseTypeMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllProviderResponseTypeMasters, () => _context.ProviderResponseTypeMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ProviderResponseTypeMaster>();
        public async Task<ProviderResponseTypeMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ProviderResponseTypeMasterID == id); }

        public async Task<int> Insert(ProviderResponseTypeMaster entity) { await _context.ProviderResponseTypeMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderResponseTypeMasters); return result; }
        public async Task<int> Update(ProviderResponseTypeMaster entity) { _context.ProviderResponseTypeMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderResponseTypeMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ProviderResponseTypeMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderResponseTypeMasters); return result; }
    }
}
