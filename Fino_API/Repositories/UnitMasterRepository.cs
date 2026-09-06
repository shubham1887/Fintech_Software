using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UnitMasterRepository : IUnitMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UnitMaster>> _listCache;
        public UnitMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UnitMaster>>(); }
        public async Task<List<UnitMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUnitMasters, () => _context.UnitMasters.AsNoTracking().ToList()) ?? new List<UnitMaster>();
        public async Task<UnitMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UnitMasterID == id); }

        public async Task<int> Insert(UnitMaster entity) { await _context.UnitMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUnitMasters); return result; }
        public async Task<int> Update(UnitMaster entity) { _context.UnitMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUnitMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UnitMasters.FindAsync(id); if (entity == null) return 0; _context.UnitMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUnitMasters); return result; }
    }
}
