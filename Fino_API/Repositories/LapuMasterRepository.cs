using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class LapuMasterRepository : ILapuMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<LapuMaster>> _listCache;
        public LapuMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<LapuMaster>>(); }
        public async Task<List<LapuMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllLapuMasters, () => _context.LapuMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<LapuMaster>();
        public async Task<LapuMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.LapuMasterID == id); }

        public async Task<int> Insert(LapuMaster entity) { await _context.LapuMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLapuMasters); return result; }
        public async Task<int> Update(LapuMaster entity) { _context.LapuMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLapuMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.LapuMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLapuMasters); return result; }
    }
}
