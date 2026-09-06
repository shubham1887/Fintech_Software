using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class InboxRepository : IInboxRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<Inbox>> _listCache;
        public InboxRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<Inbox>>(); }
        public async Task<List<Inbox>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllInboxes, () => _context.Inboxes.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<Inbox>();
        public async Task<Inbox?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.InboxID == id); }
        public async Task<List<Inbox>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(Inbox entity) { await _context.Inboxes.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllInboxes); return result; }
        public async Task<int> Update(Inbox entity) { _context.Inboxes.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllInboxes); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.Inboxes.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllInboxes); return result; }
    }
}
