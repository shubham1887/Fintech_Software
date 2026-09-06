using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UpiSenderRepository : IUpiSenderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UpiSender>> _listCache;
        public UpiSenderRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UpiSender>>(); }
        public async Task<List<UpiSender>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUpiSenders, () => _context.UpiSenders.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<UpiSender>();
        public async Task<UpiSender?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UpiSenderID == id); }
        public async Task<List<UpiSender>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(UpiSender entity) { await _context.UpiSenders.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUpiSenders); return result; }
        public async Task<int> Update(UpiSender entity) { _context.UpiSenders.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUpiSenders); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UpiSenders.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUpiSenders); return result; }
    }
}
