using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class PureDMTTransferBeneDetailRepository : IPureDMTTransferBeneDetailRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PureDMTTransferBeneDetail>> _listCache;
        public PureDMTTransferBeneDetailRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<PureDMTTransferBeneDetail>>(); }
        public async Task<List<PureDMTTransferBeneDetail>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllPureDMTTransferBeneDetails, () => _context.PureDMTTransferBeneDetails.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<PureDMTTransferBeneDetail>();
        public async Task<PureDMTTransferBeneDetail?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.PureDMTTransferBeneDetailID == id); }

        public async Task<int> Insert(PureDMTTransferBeneDetail entity) { await _context.PureDMTTransferBeneDetails.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPureDMTTransferBeneDetails); return result; }
        public async Task<int> Update(PureDMTTransferBeneDetail entity) { _context.PureDMTTransferBeneDetails.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPureDMTTransferBeneDetails); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.PureDMTTransferBeneDetails.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllPureDMTTransferBeneDetails); return result; }
    }
}
