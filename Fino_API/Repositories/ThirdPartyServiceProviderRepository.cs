using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class ThirdPartyServiceProviderRepository : IThirdPartyServiceProviderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ThirdPartyServiceProvider>> _listCache;
        public ThirdPartyServiceProviderRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ThirdPartyServiceProvider>>(); }
        public async Task<List<ThirdPartyServiceProvider>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllThirdPartyServiceProviders, () => _context.ThirdPartyServiceProviders.AsNoTracking().ToList()) ?? new List<ThirdPartyServiceProvider>();
        public async Task<ThirdPartyServiceProvider?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ThirdPartyServiceProviderID == id); }

        public async Task<int> Insert(ThirdPartyServiceProvider entity) { await _context.ThirdPartyServiceProviders.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllThirdPartyServiceProviders); return result; }
        public async Task<int> Update(ThirdPartyServiceProvider entity) { _context.ThirdPartyServiceProviders.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllThirdPartyServiceProviders); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ThirdPartyServiceProviders.FindAsync(id); if (entity == null) return 0; _context.ThirdPartyServiceProviders.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllThirdPartyServiceProviders); return result; }
    }
}
