using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class DMTBankRepository : IDMTBankRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<DMTBank>> _listCache;

        public DMTBankRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<DMTBank>>();
        }

        public async Task<List<DMTBank>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllDMTBanks, () =>
                _context.DMTBanks.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToList()
            ) ?? new List<DMTBank>();
        }

        public async Task<DMTBank?> GetByID(int dmtBankId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.DMTBankID == dmtBankId);
        }

        public async Task<DMTBank?> GetByName(string name)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> Insert(DMTBank entity)
        {
            await _context.DMTBanks.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDMTBanks);
            return result;
        }

        public async Task<int> Update(DMTBank entity)
        {
            _context.DMTBanks.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDMTBanks);
            return result;
        }

        public async Task<int> Delete(int dmtBankId)
        {
            var entity = await _context.DMTBanks.FindAsync(dmtBankId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllDMTBanks);
            return result;
        }
    }
}
