using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    /// <summary>
    /// The rewrite rules, cached as one list.
    ///
    /// Every user-facing message passes through these, so the lookup happens on effectively every
    /// response the API sends. Reading the table each time would add a query to every single call;
    /// the list is tiny and changes only when an admin edits it, so it is cached and dropped on
    /// write — the same shape as every other master in this layer.
    /// </summary>
    public class ResponseMessageRuleRepository : IResponseMessageRuleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<ResponseMessageRule>> _listCache;

        public ResponseMessageRuleRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<ResponseMessageRule>>();
        }

        public async Task<List<ResponseMessageRule>> GetAll() =>
            _listCache.GetOrCreate(CacheKeys.AllResponseMessageRules, () =>
                _context.ResponseMessageRules.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Sequence)
                    .ThenBy(x => x.ResponseMessageRuleID)
                    .ToList()) ?? new List<ResponseMessageRule>();

        public async Task<ResponseMessageRule?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.ResponseMessageRuleID == id);
        }

        public async Task<int> Insert(ResponseMessageRule entity)
        {
            await _context.ResponseMessageRules.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllResponseMessageRules);
            return result;
        }

        public async Task<int> Update(ResponseMessageRule entity)
        {
            _context.ResponseMessageRules.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllResponseMessageRules);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.ResponseMessageRules.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.Now;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllResponseMessageRules);
            return result;
        }
    }
}
