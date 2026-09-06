using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ProviderRequestResponseLogRepository : IProviderRequestResponseLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ProviderRequestResponseLog>> _listCache;
        public ProviderRequestResponseLogRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ProviderRequestResponseLog>>(); }
        public async Task<List<ProviderRequestResponseLog>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllProviderRequestResponseLogs, () => _context.ProviderRequestResponseLogs.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<ProviderRequestResponseLog>();
        public async Task<ProviderRequestResponseLog?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ProviderRequestResponseLogID == id); }
        public async Task<List<ProviderRequestResponseLog>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(ProviderRequestResponseLog entity)
        {
            await _context.ProviderRequestResponseLogs.AddAsync(entity);
            try
            {
                int result = await _context.SaveChangesAsync();
                if (result > 0) _listCache.Remove(CacheKeys.AllProviderRequestResponseLogs);
                return result;
            }
            catch
            {
                // A failed log row must not stay tracked — the scoped context is shared and a
                // poisoned entry would break every later SaveChanges in the same request.
                _context.Entry(entity).State = EntityState.Detached;
                throw;
            }
        }
        public async Task<int> Update(ProviderRequestResponseLog entity) { _context.ProviderRequestResponseLogs.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderRequestResponseLogs); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ProviderRequestResponseLogs.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllProviderRequestResponseLogs); return result; }
    }
}
