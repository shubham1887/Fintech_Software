using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ChannelSlabMasterRepository : IChannelSlabMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ChannelSlabMaster>> _listCache;

        public ChannelSlabMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<ChannelSlabMaster>>();
        }

        public async Task<List<ChannelSlabMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllChannelSlabMasters, () =>
                _context.ChannelSlabMasters.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.SlabName).ToList()
            ) ?? new List<ChannelSlabMaster>();
        }

        public async Task<List<ChannelSlabMaster>> GetByUplineUser(int uplineUserId)
        {
            var all = await GetAll();
            return all.Where(x => x.UplineUserID == uplineUserId).ToList();
        }

        public async Task<ChannelSlabMaster?> GetByID(int channelSlabMasterId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.ChannelSlabMasterID == channelSlabMasterId);
        }

        public async Task<int> Insert(ChannelSlabMaster entity)
        {
            await _context.ChannelSlabMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllChannelSlabMasters);
            return result;
        }

        public async Task<int> Update(ChannelSlabMaster entity)
        {
            _context.ChannelSlabMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllChannelSlabMasters);
            return result;
        }

        public async Task<int> Delete(int channelSlabMasterId)
        {
            var entity = await _context.ChannelSlabMasters.FindAsync(channelSlabMasterId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllChannelSlabMasters);
            return result;
        }
    }
}
