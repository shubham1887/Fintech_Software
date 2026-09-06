using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class VerificationDocumentConfigRepository : IVerificationDocumentConfigRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<VerificationDocumentConfig>> _listCache;

        public VerificationDocumentConfigRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<VerificationDocumentConfig>>();
        }

        public async Task<List<VerificationDocumentConfig>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllVerificationDocumentConfigs, () =>
                _context.VerificationDocumentConfigs.AsNoTracking()
                    .OrderBy(x => x.DisplayOrder).ThenBy(x => x.VerificationDocumentConfigID).ToList()
            ) ?? new List<VerificationDocumentConfig>();
        }

        public async Task<VerificationDocumentConfig?> GetByID(int id)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.VerificationDocumentConfigID == id);
        }

        public async Task<int> Insert(VerificationDocumentConfig entity)
        {
            await _context.VerificationDocumentConfigs.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVerificationDocumentConfigs);
            return result;
        }

        public async Task<int> Update(VerificationDocumentConfig entity)
        {
            _context.VerificationDocumentConfigs.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVerificationDocumentConfigs);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.VerificationDocumentConfigs.FindAsync(id);
            if (entity == null) return 0;
            _context.VerificationDocumentConfigs.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllVerificationDocumentConfigs);
            return result;
        }
    }
}
