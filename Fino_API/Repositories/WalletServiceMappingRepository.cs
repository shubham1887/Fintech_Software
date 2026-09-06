using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class WalletServiceMappingRepository : IWalletServiceMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<WalletServiceMapping>> _listCache;
        public WalletServiceMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<WalletServiceMapping>>(); }
        public async Task<List<WalletServiceMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllWalletServiceMappings, () => _context.WalletServiceMappings.AsNoTracking().Include(x => x.WalletMaster).Include(x => x.ServiceMaster).Where(x => !x.IsDeleted).ToList()) ?? new List<WalletServiceMapping>();
        public async Task<WalletServiceMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.WalletServiceMappingID == id); }

        public async Task<int> Insert(WalletServiceMapping entity) { await _context.WalletServiceMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllWalletServiceMappings); return result; }
        public async Task<int> Update(WalletServiceMapping entity)
        {
            // Load a FRESH tracked row (no Includes) and set scalars only. Updating the detached,
            // AsNoTracking graph from GetAll() carried a stale WalletMaster navigation whose key
            // overrode the new WalletID during EF fix-up — so the wallet change silently reverted.
            var tracked = await _context.WalletServiceMappings
                .FirstOrDefaultAsync(x => x.WalletServiceMappingID == entity.WalletServiceMappingID && !x.IsDeleted);
            if (tracked == null) return 0;
            tracked.WalletID = entity.WalletID;
            tracked.ServiceID = entity.ServiceID;
            tracked.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWalletServiceMappings);
            return result;
        }
        public async Task<int> Delete(int id) { var entity = await _context.WalletServiceMappings.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllWalletServiceMappings); return result; }
    }
}
