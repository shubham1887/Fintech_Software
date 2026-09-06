using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class SenderRepository : ISenderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<Sender>> _listCache;
        public SenderRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<Sender>>(); }
        public async Task<List<Sender>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllSenders, () => _context.Senders.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<Sender>();
        public async Task<Sender?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.SenderID == id); }
        public async Task<List<Sender>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(Sender entity) { await _context.Senders.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSenders); return result; }
        public async Task<int> Update(Sender entity) { _context.Senders.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSenders); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.Senders.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllSenders); return result; }
    }
}
