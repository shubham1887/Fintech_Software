using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class BulkMessageRepository : IBulkMessageRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<BulkMessage>> _listCache;
        public BulkMessageRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<BulkMessage>>(); }
        public async Task<List<BulkMessage>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllBulkMessages, () => _context.BulkMessages.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<BulkMessage>();
        public async Task<BulkMessage?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.BulkMessageID == id); }
        public async Task<List<BulkMessage>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(BulkMessage entity) { await _context.BulkMessages.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBulkMessages); return result; }
        public async Task<int> Update(BulkMessage entity) { _context.BulkMessages.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBulkMessages); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.BulkMessages.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllBulkMessages); return result; }
    }
}
