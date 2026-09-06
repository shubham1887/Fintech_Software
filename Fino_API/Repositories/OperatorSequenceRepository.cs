using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class OperatorSequenceRepository : IOperatorSequenceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OperatorSequence>> _listCache;
        public OperatorSequenceRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<OperatorSequence>>(); }
        public async Task<List<OperatorSequence>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllOperatorSequences, () => _context.OperatorSequences.AsNoTracking().Where(x => !x.IsDeleted).Include(x => x.ServiceMaster).Include(x => x.OperatorMaster).Include(x => x.ProviderMaster).ToList()) ?? new List<OperatorSequence>();
        public async Task<OperatorSequence?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.OperatorSequenceID == id); }
        public async Task<List<OperatorSequence>> GetByOperatorID(int operatorID) { var list = await GetAll(); return list.Where(x => x.OperatorID == operatorID).ToList(); }
        public async Task<int> Insert(OperatorSequence entity) { await _context.OperatorSequences.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperatorSequences); return result; }
        public async Task<int> Update(OperatorSequence entity) { _context.OperatorSequences.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperatorSequences); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.OperatorSequences.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllOperatorSequences); return result; }
        public async Task<OperatorSequence?> GetByOperatorAndProvider(int operatorID, int providerID) { var list = await GetAll(); return list.FirstOrDefault(x => x.OperatorID == operatorID && x.ProviderID == providerID); }
        public async Task<int> DeleteByOperatorID(int operatorID)
        {
            var rows = await _context.OperatorSequences.Where(x => x.OperatorID == operatorID && !x.IsDeleted).ToListAsync();
            if (rows.Count == 0) return 0;
            foreach (var row in rows) { row.IsDeleted = true; row.UpdatedateTime = DateTime.UtcNow; }
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorSequences);
            return result;
        }
        public async Task<int> DeleteByOperatorAndProvider(int operatorID, int providerID)
        {
            var rows = await _context.OperatorSequences.Where(x => x.OperatorID == operatorID && x.ProviderID == providerID && !x.IsDeleted).ToListAsync();
            if (rows.Count == 0) return 0;
            foreach (var row in rows) { row.IsDeleted = true; row.UpdatedateTime = DateTime.UtcNow; }
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorSequences);
            return result;
        }
    }
}
