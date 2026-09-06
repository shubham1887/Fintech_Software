using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class ProviderOperatorCircleMasterRepository : IProviderOperatorCircleMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderOperatorCircleMaster>> _listCache;
        public ProviderOperatorCircleMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ProviderOperatorCircleMaster>>(); }
        public async Task<List<ProviderOperatorCircleMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllProviderOperatorCircleMasters, () => _context.ProviderOperatorCircleMasters.AsNoTracking().ToList()) ?? new List<ProviderOperatorCircleMaster>();
        public async Task<ProviderOperatorCircleMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ProviderOperatorCircleMasterID == id); }

        public async Task<int> Insert(ProviderOperatorCircleMaster entity) { await _context.ProviderOperatorCircleMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderOperatorCircleMasters); return result; }
        public async Task<int> Update(ProviderOperatorCircleMaster entity) { _context.ProviderOperatorCircleMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderOperatorCircleMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ProviderOperatorCircleMasters.FindAsync(id); if (entity == null) return 0; _context.ProviderOperatorCircleMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderOperatorCircleMasters); return result; }
    }
}
