using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class SubModeRepository : ISubModeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<SubMode>> _listCache;

        public SubModeRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<SubMode>>();
        }

        public async Task<List<SubMode>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllSubModes, () =>
                _context.SubModes
                    .AsNoTracking()
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.SubModeName)
                    .ToList()
            ) ?? new List<SubMode>();
        }

        public async Task<List<SubMode>> GetByModeID(int modeId)
        {
            var all = await GetAll();
            return all.Where(x => x.ModeID == modeId).ToList();
        }

        public async Task<SubMode?> GetByID(int subModeId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.SubModeID == subModeId);
        }

        public async Task<bool> ExistsByName(int modeId, string subModeName, int excludeSubModeId)
        {
            var all = await GetAll();
            return all.Any(x => x.ModeID == modeId
                && x.SubModeID != excludeSubModeId
                && x.SubModeName.Trim().Equals(subModeName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> AnyByModeID(int modeId)
        {
            var all = await GetAll();
            return all.Any(x => x.ModeID == modeId);
        }

        public async Task<int> Insert(SubMode entity)
        {
            await _context.SubModes.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSubModes);
            return result;
        }

        public async Task<int> Update(SubMode entity)
        {
            _context.SubModes.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSubModes);
            return result;
        }

        public async Task<int> Delete(int subModeId)
        {
            var entity = await _context.SubModes.FindAsync(subModeId);
            if (entity == null) return 0;
            entity.IsDelete = true;
            entity.UpdatedDate = DateTime.Now;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllSubModes);
            return result;
        }
    }
}
