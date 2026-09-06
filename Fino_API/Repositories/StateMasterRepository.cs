using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class StateMasterRepository : IStateMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<StateMaster>> _listCache;

        public StateMasterRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<StateMaster>>();
        }

        public async Task<List<StateMaster>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllStates, () =>
                _context.StateMasters.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.StateName).ToList()
            ) ?? new List<StateMaster>();
        }

        public async Task<StateMaster?> GetByID(int stateId)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.StateID == stateId);
        }

        public async Task<StateMaster?> GetByName(string stateName)
        {
            var all = await GetAll();
            return all.FirstOrDefault(x => x.StateName.Equals(stateName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> Insert(StateMaster entity)
        {
            await _context.StateMasters.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllStates);
            return result;
        }

        public async Task<int> Update(StateMaster entity)
        {
            _context.StateMasters.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllStates);
            return result;
        }

        public async Task<int> Delete(int stateId)
        {
            var entity = await _context.StateMasters.FindAsync(stateId);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllStates);
            return result;
        }
    }
}
