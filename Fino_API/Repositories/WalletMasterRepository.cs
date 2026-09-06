using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class WalletMasterRepository : IWalletMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<WalletMaster>> _listCache;

        public WalletMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<WalletMaster>>();
        }

        public async Task<List<WalletMaster>> SelectWithoutCache()
        {
            return _listCache.GetOrCreate(CacheKeys.WalletMaster, () =>
            {
                return _context.WalletMaster
                .OrderBy(x=>x.UISequence)
                .Where(x=>x.IsActive)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<WalletMaster>();
        }
        public async Task<List<WalletMaster>> Select()
        {
            return await SelectWithoutCache();
        }

        public async Task<WalletMaster> SelectByID(int WalletID)
        {
            List<WalletMaster> list = await SelectWithoutCache();

            WalletMaster item = list.FirstOrDefault(C => C.WalletID == WalletID);

            return item;
        }

        public async Task<List<WalletMaster>> SelectAll()
        {
            return await _context.WalletMaster
                .OrderBy(x => x.UISequence)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<WalletMaster?> GetForUpdate(int WalletID)
        {
            return await _context.WalletMaster.FirstOrDefaultAsync(x => x.WalletID == WalletID);
        }

        public async Task<int> Update(WalletMaster wallet)
        {
            _context.WalletMaster.Update(wallet);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.WalletMaster);
            return result;
        }
    }
}
