using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class LocationLookUpRepository : ILocationLookUpRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<LocationLookUp>> _listCache;
        public LocationLookUpRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<LocationLookUp>>(); }
        public async Task<List<LocationLookUp>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllLocationLookUps, () => _context.LocationLookUps.AsNoTracking().ToList()) ?? new List<LocationLookUp>();
        public async Task<LocationLookUp?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.LocationLookUpID == id); }

        public async Task<int> Insert(LocationLookUp entity) { await _context.LocationLookUps.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLocationLookUps); return result; }
        public async Task<int> Update(LocationLookUp entity) { _context.LocationLookUps.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLocationLookUps); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.LocationLookUps.FindAsync(id); if (entity == null) return 0; _context.LocationLookUps.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLocationLookUps); return result; }
    }
}
