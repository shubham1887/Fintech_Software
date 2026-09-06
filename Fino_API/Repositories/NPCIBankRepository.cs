using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class NPCIBankRepository : INPCIBankRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<NPCIBank>> _listCache;

        public NPCIBankRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<NPCIBank>>();
        }

        public async Task<List<NPCIBank>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllNPCIBanks, () =>
                _context.NPCIBanks.AsNoTracking().ToList()
            ) ?? new List<NPCIBank>();
        }

        public async Task<NPCIBank?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.NPCIBankID == id);
        }

        public async Task<int> Insert(NPCIBank entity)
        {
            await _context.NPCIBanks.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllNPCIBanks);
            return result;
        }

        public async Task<int> Update(NPCIBank entity)
        {
            _context.NPCIBanks.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllNPCIBanks);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.NPCIBanks.FindAsync(id);
            if (entity == null) return 0;
            _context.NPCIBanks.Remove(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllNPCIBanks);
            return result;
        }
    }
}
