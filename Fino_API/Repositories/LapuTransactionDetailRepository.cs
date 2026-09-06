using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class LapuTransactionDetailRepository : ILapuTransactionDetailRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<LapuTransactionDetail>> _listCache;
        public LapuTransactionDetailRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<LapuTransactionDetail>>(); }
        public async Task<List<LapuTransactionDetail>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllLapuTransactionDetails, () => _context.LapuTransactionDetails.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<LapuTransactionDetail>();
        public async Task<LapuTransactionDetail?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.LapuTransactionDetailID == id); }
        public async Task<List<LapuTransactionDetail>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(LapuTransactionDetail entity) { await _context.LapuTransactionDetails.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLapuTransactionDetails); return result; }
        public async Task<int> Update(LapuTransactionDetail entity) { _context.LapuTransactionDetails.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLapuTransactionDetails); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.LapuTransactionDetails.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllLapuTransactionDetails); return result; }
    }
}
