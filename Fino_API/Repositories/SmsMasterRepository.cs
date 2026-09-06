using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class SmsMasterRepository : ISmsMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SmsMaster>> _listCache;

        public SmsMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<SmsMaster>>();
        }

        public async Task<List<SmsMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllSmsMasters, () =>
                _context.SmsMasters
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<SmsMaster>();
        }

        public async Task<SmsMaster?> GetByID(int smsMasterId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.SMSMasterID == smsMasterId);
        }

        public async Task<int> Insert(SmsMaster entity)
        {
            await _context.SmsMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSmsMasters);
            return result;
        }

        public async Task<int> Update(SmsMaster entity)
        {
            _context.SmsMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSmsMasters);
            return result;
        }

        public async Task<int> Delete(int smsMasterId)
        {
            var entity = await _context.SmsMasters.FindAsync(smsMasterId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSmsMasters);
            return result;
        }
    }
}
