using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class LapuPartyMasterRepository : ILapuPartyMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<LapuPartyMaster>> _listCache;
        public LapuPartyMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<LapuPartyMaster>>(); }
        public async Task<List<LapuPartyMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllLapuPartyMasters, () => _context.LapuPartyMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<LapuPartyMaster>();
        public async Task<LapuPartyMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.LapuPartyMasterID == id); }

        public async Task<int> Insert(LapuPartyMaster entity) { await _context.LapuPartyMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLapuPartyMasters); return result; }
        public async Task<int> Update(LapuPartyMaster entity) { _context.LapuPartyMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLapuPartyMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.LapuPartyMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLapuPartyMasters); return result; }
    }
}
