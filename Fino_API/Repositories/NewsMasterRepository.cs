using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class NewsMasterRepository : INewsMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<NewsMaster>> _listCache;

        public NewsMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<NewsMaster>>();
        }

        public async Task<List<NewsMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllNews, () =>
                _context.NewsMasters
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .ToList()
            ) ?? new List<NewsMaster>();
        }

        public async Task<NewsMaster?> GetByID(int newsId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.NewsID == newsId);
        }

        public async Task<int> Insert(NewsMaster entity)
        {
            await _context.NewsMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllNews);
            return result;
        }

        public async Task<int> Update(NewsMaster entity)
        {
            _context.NewsMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllNews);
            return result;
        }

        public async Task<int> Delete(int newsId)
        {
            var entity = await _context.NewsMasters.FindAsync(newsId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllNews);
            return result;
        }
    }
}
