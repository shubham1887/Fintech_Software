using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class RemainderOperatorAmountWiseRepository : IRemainderOperatorAmountWiseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<RemainderOperatorAmountWise>> _listCache;
        public RemainderOperatorAmountWiseRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<RemainderOperatorAmountWise>>(); }
        public async Task<List<RemainderOperatorAmountWise>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllRemainderOperatorAmountWises, () => _context.RemainderOperatorAmountWises.AsNoTracking().ToList()) ?? new List<RemainderOperatorAmountWise>();
        public async Task<RemainderOperatorAmountWise?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.RemainderOperatorAmountWiseID == id); }

        public async Task<int> Insert(RemainderOperatorAmountWise entity) { await _context.RemainderOperatorAmountWises.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRemainderOperatorAmountWises); return result; }
        public async Task<int> Update(RemainderOperatorAmountWise entity) { _context.RemainderOperatorAmountWises.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRemainderOperatorAmountWises); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.RemainderOperatorAmountWises.FindAsync(id); if (entity == null) return 0; _context.RemainderOperatorAmountWises.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllRemainderOperatorAmountWises); return result; }
    }
}
