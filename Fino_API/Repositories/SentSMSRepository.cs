using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class SentSMSRepository : ISentSMSRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SentSMS>> _listCache;
        public SentSMSRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<SentSMS>>(); }
        public async Task<List<SentSMS>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSentSMS, () => _context.SentSMS.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<SentSMS>();
        public async Task<SentSMS?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SentSMSID == id); }
        public async Task<List<SentSMS>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(SentSMS entity) { await _context.SentSMS.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSentSMS); return result; }
        public async Task<int> Update(SentSMS entity) { _context.SentSMS.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSentSMS); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.SentSMS.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSentSMS); return result; }
    }
}
