using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class OperatorSequenceUserWiseRepository : IOperatorSequenceUserWiseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OperatorSequenceUserWise>> _listCache;

        public OperatorSequenceUserWiseRepository(ApplicationDbContext context, MemoryCacheProcessing<List<OperatorSequenceUserWise>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<OperatorSequenceUserWise>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllOperatorSequenceUserWises, () =>
                _context.OperatorSequenceUserWises.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.UserMaster)
                    .Include(x => x.ServiceMaster)
                    .Include(x => x.OperatorMaster)
                    .Include(x => x.ProviderMaster)
                    .ToList()
            ) ?? new List<OperatorSequenceUserWise>();
        }

        public async Task<List<OperatorSequenceUserWise>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<OperatorSequenceUserWise?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.OperatorSequenceUserWiseID == id);
        }

        public async Task<int> Insert(OperatorSequenceUserWise entity)
        {
            await _context.OperatorSequenceUserWises.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorSequenceUserWises);
            return result;
        }

        public async Task<int> Update(OperatorSequenceUserWise entity)
        {
            _context.OperatorSequenceUserWises.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorSequenceUserWises);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.OperatorSequenceUserWises.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorSequenceUserWises);
            return result;
        }

        public async Task<List<OperatorSequenceUserWise>> GetByUserAndOperator(int userId, int operatorID)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId && x.OperatorID == operatorID)
                .OrderBy(x => x.OperatorSequenceUserWiseID)
                .ToList();
        }

        public async Task<int> DeleteByUserAndOperator(int userId, int operatorID)
        {
            var rows = await _context.OperatorSequenceUserWises
                .Where(x => x.UserID == userId && x.OperatorID == operatorID && !x.IsDeleted).ToListAsync();
            if (rows.Count == 0) return 0;
            foreach (var row in rows) { row.IsDeleted = true; row.UpdatedateTime = DateTime.UtcNow; }
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorSequenceUserWises);
            return result;
        }

        public async Task<int> DeleteAllRows()
        {
            var rows = await _context.OperatorSequenceUserWises.Where(x => !x.IsDeleted).ToListAsync();
            if (rows.Count == 0) return 0;
            foreach (var row in rows) { row.IsDeleted = true; row.UpdatedateTime = DateTime.UtcNow; }
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorSequenceUserWises);
            return result;
        }
    }
}
