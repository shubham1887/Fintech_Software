using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class WebsitePageRepository : IWebsitePageRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<WebsitePageMaster>> _listCache;

        public WebsitePageRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<WebsitePageMaster>>();
        }

        public async Task<List<WebsitePageMaster>> GetAll() =>
            _listCache.GetOrCreate(CacheKeys.AllWebsitePages,
                () => _context.WebsitePageMasters.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.MenuOrder).ToList())
            ?? new List<WebsitePageMaster>();

        public async Task<WebsitePageMaster?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.WebsitePageMasterID == id);
        }

        public async Task<int> Insert(WebsitePageMaster entity)
        {
            entity.DateTime = DateTime.Now;
            entity.UpdatedateTime = DateTime.Now;
            await _context.WebsitePageMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWebsitePages);
            return result;
        }

        public async Task<int> Update(WebsitePageMaster entity)
        {
            entity.UpdatedateTime = DateTime.Now;
            _context.WebsitePageMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWebsitePages);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.WebsitePageMasters.FindAsync(id);
            if (entity == null) return 0;
            _context.WebsitePageMasters.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllWebsitePages);
            return result;
        }
    }
}
