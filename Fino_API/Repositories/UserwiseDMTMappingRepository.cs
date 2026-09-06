using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserwiseDMTMappingRepository : IUserwiseDMTMappingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserwiseDMTMapping>> _listCache;

        public UserwiseDMTMappingRepository(ApplicationDbContext context, MemoryCacheProcessing<List<UserwiseDMTMapping>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<UserwiseDMTMapping>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUserwiseDMTMappings, () =>
                _context.UserwiseDMTMappings.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.UserMaster)
                    .Include(x => x.ServiceMaster)
                    .Include(x => x.OperatorMaster)
                    .Include(x => x.ProviderMaster)
                    .ToList()
            ) ?? new List<UserwiseDMTMapping>();
        }

        public async Task<List<UserwiseDMTMapping>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<UserwiseDMTMapping?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.UserwiseDMTMappingID == id);
        }

        public async Task<int> Insert(UserwiseDMTMapping entity)
        {
            await _context.UserwiseDMTMappings.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseDMTMappings);
            return result;
        }

        public async Task<int> Update(UserwiseDMTMapping entity)
        {
            _context.UserwiseDMTMappings.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseDMTMappings);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserwiseDMTMappings.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllUserwiseDMTMappings);
            return result;
        }
    }
}
