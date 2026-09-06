using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class DailyAuthenticationMasterRepository : IDailyAuthenticationMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<DailyAuthenticationMaster>> _listCache;
        public DailyAuthenticationMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<DailyAuthenticationMaster>>(); }
        public async Task<List<DailyAuthenticationMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllDailyAuthenticationMasters, () => _context.DailyAuthenticationMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<DailyAuthenticationMaster>();
        public async Task<DailyAuthenticationMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.AuthenticationID == id); }
        public async Task<List<DailyAuthenticationMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(DailyAuthenticationMaster entity) { await _context.DailyAuthenticationMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDailyAuthenticationMasters); return result; }
        public async Task<int> Update(DailyAuthenticationMaster entity) { _context.DailyAuthenticationMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDailyAuthenticationMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.DailyAuthenticationMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllDailyAuthenticationMasters); return result; }
    }
}
