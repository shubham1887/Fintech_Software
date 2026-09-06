using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UserwiseServiceChargeMasterRepository : IUserwiseServiceChargeMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserwiseServiceChargeMaster>> _listCache;
        public UserwiseServiceChargeMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UserwiseServiceChargeMaster>>(); }
        public async Task<List<UserwiseServiceChargeMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUserwiseServiceChargeMasters, () => _context.UserwiseServiceChargeMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UserwiseServiceChargeMaster>();
        public async Task<UserwiseServiceChargeMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UserwiseServiceChargeMasterID == id); }
        public async Task<List<UserwiseServiceChargeMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UserwiseServiceChargeMaster entity) { await _context.UserwiseServiceChargeMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseServiceChargeMasters); return result; }
        public async Task<int> Update(UserwiseServiceChargeMaster entity) { _context.UserwiseServiceChargeMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseServiceChargeMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UserwiseServiceChargeMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseServiceChargeMasters); return result; }
    }
}
