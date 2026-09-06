using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class SliderMasterRepository : ISliderMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SliderMaster>> _listCache;

        public SliderMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<SliderMaster>>();
        }

        public async Task<List<SliderMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllSliderMasters, () =>
                _context.SliderMasters
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<SliderMaster>();
        }

        public async Task<SliderMaster?> GetByID(int sliderMasterId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.SliderMasterID == sliderMasterId);
        }

        public async Task<int> Insert(SliderMaster entity)
        {
            await _context.SliderMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSliderMasters);
            return result;
        }

        public async Task<int> Update(SliderMaster entity)
        {
            _context.SliderMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSliderMasters);
            return result;
        }

        public async Task<int> Delete(int sliderMasterId)
        {
            var entity = await _context.SliderMasters.FindAsync(sliderMasterId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSliderMasters);
            return result;
        }
    }
}
