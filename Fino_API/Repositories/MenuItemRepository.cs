using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Processing;
using FintechApp.StaticModels;

namespace FintechApp.Repositories
{
    public interface IMenuItemRepository
    {
    }
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserMaster>> _listCache;

        public MenuItemRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<UserMaster>>();
        }
       
        public void ClearUserCache()
        {
            _listCache.Remove(CacheKeys.AllUsers);
        }
    }
}
