using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ChannelSlabDetailRepository : IChannelSlabDetailRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ChannelSlabDetail>> _listCache;

        public ChannelSlabDetailRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<ChannelSlabDetail>>();
        }

        public async Task<List<ChannelSlabDetail>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllChannelSlabDetails, () =>
                _context.ChannelSlabDetails
                    .Include(x => x.ChannelSlabMaster)
                    .Include(x => x.ServiceMaster)
                    .Include(x => x.OperatorMaster)
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<ChannelSlabDetail>();
        }

        public async Task<List<ChannelSlabDetail>> GetByChannelSlabMasterID(int channelSlabMasterId)
        {
            var all = await GetAll();
            return all.Where(x => x.ChannelSlabMasterID == channelSlabMasterId).ToList();
        }

        public async Task<ChannelSlabDetail?> GetByID(int channelSlabDetailId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.ChannelSlabDetailID == channelSlabDetailId);
        }

        public async Task<int> Insert(ChannelSlabDetail entity)
        {
            await _context.ChannelSlabDetails.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllChannelSlabDetails);
            return result;
        }

        public async Task<int> Update(ChannelSlabDetail entity)
        {
            _context.ChannelSlabDetails.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllChannelSlabDetails);
            return result;
        }

        public async Task<int> Delete(int channelSlabDetailId)
        {
            var entity = await _context.ChannelSlabDetails.FindAsync(channelSlabDetailId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllChannelSlabDetails);
            return result;
        }
    }
}
