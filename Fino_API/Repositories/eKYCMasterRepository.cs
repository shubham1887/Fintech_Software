using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class eKYCMasterRepository : IeKYCMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<eKYCMaster>> _listCache;
        public eKYCMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<eKYCMaster>>(); }
        public async Task<List<eKYCMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AlleKYCMasters, () => _context.eKYCMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<eKYCMaster>();
        public async Task<eKYCMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.eKYCMasterID == id); }
        public async Task<List<eKYCMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(eKYCMaster entity) { await _context.eKYCMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AlleKYCMasters); return result; }
        public async Task<int> Update(eKYCMaster entity) { _context.eKYCMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AlleKYCMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.eKYCMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AlleKYCMasters); return result; }
    }
}
