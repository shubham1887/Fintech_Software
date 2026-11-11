using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserChangeHistoryRepository : IUserChangeHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public UserChangeHistoryRepository(ApplicationDbContext context) => _context = context;

        public async Task<int> Insert(UserChangeHistory entity)
        {
            await _context.UserChangeHistorys.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> InsertRange(IEnumerable<UserChangeHistory> entities)
        {
            var list = entities.ToList();
            if (list.Count == 0) return 0;
            await _context.UserChangeHistorys.AddRangeAsync(list);
            return await _context.SaveChangesAsync();
        }

        public async Task<(int count, List<UserChangeHistory> list)> Search(
            int userId, int changedByUserId, DateTime? fromDate, DateTime? toDate, string action,
            IEnumerable<int>? allowedUserIds, int skip, int take)
        {
            var query = _context.UserChangeHistorys.AsNoTracking().Where(x => !x.IsDeleted);

            if (userId > 0) query = query.Where(x => x.UserID == userId);
            if (changedByUserId > 0) query = query.Where(x => x.ChangedByUserID == changedByUserId);
            if (allowedUserIds != null)
            {
                var ids = allowedUserIds.ToList();
                query = query.Where(x => ids.Contains(x.UserID));
            }
            if (fromDate.HasValue) query = query.Where(x => x.DateTime >= fromDate.Value.Date);
            if (toDate.HasValue)
            {
                DateTime end = toDate.Value.Date.AddDays(1);
                query = query.Where(x => x.DateTime < end);
            }
            if (!string.IsNullOrWhiteSpace(action) && !action.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                query = query.Where(x => x.Action == action);

            int count = await query.CountAsync();
            var list = await query
                .OrderByDescending(x => x.DateTime)
                .ThenByDescending(x => x.UserChangeHistoryID)
                .Skip(Math.Max(skip, 0))
                .Take(take > 0 ? take : 25)
                .ToListAsync();

            return (count, list);
        }
    }
}
