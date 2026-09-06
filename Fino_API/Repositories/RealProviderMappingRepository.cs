using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class RealProviderMappingRepository : IRealProviderMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<RealProviderMapping>> _listCache;
        public RealProviderMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<RealProviderMapping>>(); }
        public async Task<List<RealProviderMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllRealProviderMappings, () => _context.RealProviderMappings.AsNoTracking().ToList()) ?? new List<RealProviderMapping>();
        public async Task<RealProviderMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.RealProviderMappingID == id); }

        public async Task<int> Insert(RealProviderMapping entity) { await _context.RealProviderMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRealProviderMappings); return result; }
        public async Task<int> Update(RealProviderMapping entity) { _context.RealProviderMappings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRealProviderMappings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.RealProviderMappings.FindAsync(id); if (entity == null) return 0; _context.RealProviderMappings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRealProviderMappings); return result; }
    }
}
