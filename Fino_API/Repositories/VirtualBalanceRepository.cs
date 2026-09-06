using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class VirtualBalanceRepository : IVirtualBalanceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<VirtualBalance>> _listCache;
        public VirtualBalanceRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<VirtualBalance>>(); }
        public async Task<List<VirtualBalance>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllVirtualBalances, () => _context.VirtualBalances.AsNoTracking().ToList()) ?? new List<VirtualBalance>();
        public async Task<VirtualBalance?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.VirtualBalanceID == id); }

        public async Task<int> Insert(VirtualBalance entity) { await _context.VirtualBalances.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllVirtualBalances); return result; }
        public async Task<int> Update(VirtualBalance entity) { _context.VirtualBalances.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllVirtualBalances); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.VirtualBalances.FindAsync(id); if (entity == null) return 0; _context.VirtualBalances.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllVirtualBalances); return result; }
    }
}
