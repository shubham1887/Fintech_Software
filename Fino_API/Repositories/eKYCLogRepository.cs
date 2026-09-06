using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class eKYCLogRepository : IeKYCLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<eKYCLog>> _listCache;
        public eKYCLogRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<eKYCLog>>(); }
        public async Task<List<eKYCLog>> GetAll() => _listCache.GetOrCreate(CacheKeys.AlleKYCLogs, () => _context.eKYCLogs.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<eKYCLog>();
        public async Task<eKYCLog?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.eKYCLogID == id); }
        public async Task<List<eKYCLog>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(eKYCLog entity) { await _context.eKYCLogs.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AlleKYCLogs); return result; }
        public async Task<int> Update(eKYCLog entity) { _context.eKYCLogs.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AlleKYCLogs); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.eKYCLogs.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AlleKYCLogs); return result; }
    }
}
