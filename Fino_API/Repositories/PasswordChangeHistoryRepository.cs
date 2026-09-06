using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class PasswordChangeHistoryRepository : IPasswordChangeHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<PasswordChangeHistory>> _listCache;

        public PasswordChangeHistoryRepository(ApplicationDbContext context, MemoryCacheProcessing<List<PasswordChangeHistory>> listCache)
        {
            _context = context;
            _listCache = listCache;
        }

        public async Task<List<PasswordChangeHistory>> GetAll()
        {
            return _listCache.GetOrCreate(CacheKeys.AllPasswordChangeHistories, () =>
                _context.PasswordChangeHistories.AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Include(x => x.UserMaster)
                    .ToList()
            ) ?? new List<PasswordChangeHistory>();
        }

        public async Task<List<PasswordChangeHistory>> GetByUserID(int userId)
        {
            var list = await GetAll();
            return list.Where(x => x.UserID == userId).ToList();
        }

        public async Task<PasswordChangeHistory?> GetByID(int id)
        {
            var list = await GetAll();
            return list.FirstOrDefault(x => x.PasswordChangeHistoryID == id);
        }

        public async Task<int> Insert(PasswordChangeHistory entity)
        {
            await _context.PasswordChangeHistories.AddAsync(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPasswordChangeHistories);
            return result;
        }

        public async Task<int> Update(PasswordChangeHistory entity)
        {
            _context.PasswordChangeHistories.Update(entity);
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPasswordChangeHistories);
            return result;
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.PasswordChangeHistories.FindAsync(id);
            if (entity == null) return 0;
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            int result = await _context.SaveChangesAsync();
            if (result > 0) _listCache.Remove(CacheKeys.AllPasswordChangeHistories);
            return result;
        }

        public async Task<int> GetCountByUserIDAndType(int userId, string type)
        {
            var list = await GetByUserID(userId);
            return list.Count(x => x.Type == type);
        }
    }
}
