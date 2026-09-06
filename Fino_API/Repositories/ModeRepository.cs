using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class ModeRepository : IModeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<Mode>> _listCache;

        public ModeRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<Mode>>();
        }

        public async Task<List<Mode>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllModes, () =>
                _context.Modes
                    .AsNoTracking()
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.ModeName)
                    .ToList()
            ) ?? new List<Mode>();
        }

        public async Task<Mode?> GetByID(int modeId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.ModeID == modeId);
        }

        public async Task<bool> ExistsByName(string modeName, int excludeModeId)
        {
            var all = await GetAll();
            return all.Any(x => x.ModeID != excludeModeId
                && x.ModeName.Trim().Equals(modeName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> Insert(Mode entity)
        {
            await _context.Modes.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllModes);
            return result;
        }

        public async Task<int> Update(Mode entity)
        {
            _context.Modes.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllModes);
            return result;
        }

        public async Task<int> Delete(int modeId)
        {
            var entity = await _context.Modes.FindAsync(modeId);
            if (entity == null) return 0;
            entity.IsDelete = true;
            entity.UpdatedDate = DateTime.Now;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllModes);
            return result;
        }
    }
}
