using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class ConsumerTypeMasterRepository : IConsumerTypeMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ConsumerTypeMaster>> _listCache;
        public ConsumerTypeMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<ConsumerTypeMaster>>(); }
        public async Task<List<ConsumerTypeMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllConsumerTypeMasters, () => _context.ConsumerTypeMasters.AsNoTracking().ToList()) ?? new List<ConsumerTypeMaster>();
        public async Task<ConsumerTypeMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.ConsumerTypeMasterID == id); }

        public async Task<int> Insert(ConsumerTypeMaster entity) { await _context.ConsumerTypeMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllConsumerTypeMasters); return result; }
        public async Task<int> Update(ConsumerTypeMaster entity) { _context.ConsumerTypeMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllConsumerTypeMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.ConsumerTypeMasters.FindAsync(id); if (entity == null) return 0; _context.ConsumerTypeMasters.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllConsumerTypeMasters); return result; }
    }
}
