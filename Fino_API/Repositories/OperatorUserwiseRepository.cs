using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class OperatorUserwiseRepository : IOperatorUserwiseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<OperatorUserwise>> _listCache;

        public OperatorUserwiseRepository(ApplicationDbContext context, MemoryCacheProcessing<List<OperatorUserwise>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<OperatorUserwise>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllOperatorUserwises, () =>
                _context.OperatorUserwises.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.UserMaster)
                    .Include(x => x.OperatorMaster)
                    .Include(x => x.ServiceMaster)
                    .ToList()
            ) ?? new List<OperatorUserwise>();
        }

        public async Task<List<OperatorUserwise>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<OperatorUserwise?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.OperatorUserwiseID == id);
        }

        public async Task<int> Insert(OperatorUserwise entity)
        {
            await _context.OperatorUserwises.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorUserwises);
            return result;
        }

        public async Task<int> Update(OperatorUserwise entity)
        {
            _context.OperatorUserwises.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorUserwises);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.OperatorUserwises.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllOperatorUserwises);
            return result;
        }
    }
}
