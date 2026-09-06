using FintechApp.Processing;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class DMTSenderRepository : IDMTSenderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<DMTSender>> _listCache;

        public DMTSenderRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<DMTSender>>();
        }

        public async Task<List<DMTSender>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllDMTSenders, () =>
                _context.DMTSenders.AsNoTracking().Where(x => !x.IsDeleted).ToList()
            ) ?? new List<DMTSender>();
        }

        public async Task<DMTSender?> GetByID(int id)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.SenderID == id);
        }

        public async Task<DMTSender?> GetByMobile(string mobile)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.Mobile == mobile);
        }

        public async Task<List<DMTSender>> GetByUserID(int userId)
        {
            var all = await GetAll();
            return all.Where(x => x.UserID == userId).ToList();
        }

        public async Task<int> Insert(DMTSender entity)
        {
            await _context.DMTSenders.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDMTSenders);
            return result;
        }

        public async Task<int> Update(DMTSender entity)
        {
            _context.DMTSenders.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDMTSenders);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.DMTSenders.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDMTSenders);
            return result;
        }
    }
}
