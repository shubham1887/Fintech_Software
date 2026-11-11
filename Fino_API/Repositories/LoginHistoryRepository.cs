using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class LoginHistoryRepository : ILoginHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public LoginHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LoginHistory>> GetByUserID(int userId, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
            => await GetFiltered(userId, fromDate, toDate, string.Empty, null, pageNumber, pageSize);

        /// <summary>
        /// Login history, filtered in the database rather than after paging.
        ///
        /// Two things were wrong with the old query. The To date was compared with
        /// <c>x.Date &lt;= toDate</c>, and the screen sends a DATE — midnight — so only a login
        /// recorded at exactly 00:00:00 on that day passed: choosing "to = today" hid every login
        /// made today. And the channel filter ran in the service AFTER this method had already
        /// paged, so filtering by Web/App silently dropped rows out of the page instead of
        /// selecting from the whole set. Both are fixed here: the To date covers the whole day, and
        /// every filter is applied before Skip/Take.
        /// </summary>
        public async Task<List<LoginHistory>> GetFiltered(
            int userId, DateTime? fromDate, DateTime? toDate, string through, IEnumerable<int>? allowedUserIds,
            int pageNumber, int pageSize)
        {
            var query = _context.LoginHistories.AsNoTracking().Where(x => !x.IsDeleted);

            if (userId > 0)
                query = query.Where(x => x.UserID == userId);

            // Visibility: a caller who is not an admin only ever sees their own network.
            if (allowedUserIds != null)
            {
                var ids = allowedUserIds.ToList();
                query = query.Where(x => ids.Contains(x.UserID));
            }

            if (fromDate.HasValue)
                query = query.Where(x => x.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
            {
                DateTime exclusiveEnd = toDate.Value.Date.AddDays(1);
                query = query.Where(x => x.Date < exclusiveEnd);
            }

            if (!string.IsNullOrWhiteSpace(through))
                query = query.Where(x => x.Through == through);

            int size = pageSize > 0 ? pageSize : 200;
            int skip = Math.Max(pageNumber - 1, 0) * size;

            return await query
                .OrderByDescending(x => x.Date)
                .Skip(skip)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> Insert(LoginHistory entity)
        {
            await _context.LoginHistories.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }
    }
}
