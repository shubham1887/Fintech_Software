using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class VirtualAccountMasterRepository : IVirtualAccountMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<VirtualAccountMaster>> _listCache;
        public VirtualAccountMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<VirtualAccountMaster>>(); }
        public async Task<List<VirtualAccountMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllVirtualAccountMasters, () => _context.VirtualAccountMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<VirtualAccountMaster>();
        public async Task<VirtualAccountMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.VirtualAccountMasterID == id); }

        public async Task<int> Insert(VirtualAccountMaster entity) { await _context.VirtualAccountMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllVirtualAccountMasters); return result; }
        public async Task<int> Update(VirtualAccountMaster entity) { _context.VirtualAccountMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllVirtualAccountMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.VirtualAccountMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllVirtualAccountMasters); return result; }
    }
}
