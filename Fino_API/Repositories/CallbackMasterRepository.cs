using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class CallbackMasterRepository : ICallbackMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CallbackMaster>> _listCache;

        public CallbackMasterRepository(ApplicationDbContext context, MemoryCacheProcessing<List<CallbackMaster>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<CallbackMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllCallbackMasters, () =>
                _context.CallbackMasters.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.UserMaster)
                    .Include(x => x.ServiceMaster)
                    .ToList()
            ) ?? new List<CallbackMaster>();
        }

        public async Task<List<CallbackMaster>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<CallbackMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.CallbackMasterID == id);
        }

        public async Task<int> Insert(CallbackMaster entity)
        {
            await _context.CallbackMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllCallbackMasters);
            return result;
        }

        public async Task<int> Update(CallbackMaster entity)
        {
            _context.CallbackMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllCallbackMasters);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.CallbackMasters.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllCallbackMasters);
            return result;
        }
    }
}
