using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class UPITransferSettingRepository : IUPITransferSettingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UPITransferSetting>> _listCache;
        public UPITransferSettingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<UPITransferSetting>>(); }
        public async Task<List<UPITransferSetting>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllUPITransferSettings, () => _context.UPITransferSettings.AsNoTracking().ToList()) ?? new List<UPITransferSetting>();
        public async Task<UPITransferSetting?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.UPITransferSettingID == id); }

        public async Task<int> Insert(UPITransferSetting entity) { await _context.UPITransferSettings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUPITransferSettings); return result; }
        public async Task<int> Update(UPITransferSetting entity) { _context.UPITransferSettings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUPITransferSettings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.UPITransferSettings.FindAsync(id); if (entity == null) return 0; _context.UPITransferSettings.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllUPITransferSettings); return result; }
    }
}
