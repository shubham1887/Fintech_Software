using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class OperatorStateMappingRepository : IOperatorStateMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OperatorStateMapping>> _listCache;
        public OperatorStateMappingRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<OperatorStateMapping>>(); }
        public async Task<List<OperatorStateMapping>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllOperatorStateMappings, () => _context.OperatorStateMappings.AsNoTracking().Where(x => !x.IsDeleted).Include(x => x.OperatorMaster).ToList()) ?? new List<OperatorStateMapping>();
        public async Task<OperatorStateMapping?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.OperatorStateMappingID == id); }
        public async Task<List<OperatorStateMapping>> GetByOperatorID(int operatorID) { var list = await GetAll(); return list.Where(x => x.OperatorID == operatorID).ToList(); }
        public async Task<int> Insert(OperatorStateMapping entity) { await _context.OperatorStateMappings.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperatorStateMappings); return result; }
        public async Task<int> Update(OperatorStateMapping entity) { _context.OperatorStateMappings.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperatorStateMappings); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.OperatorStateMappings.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperatorStateMappings); return result; }
    }
}
