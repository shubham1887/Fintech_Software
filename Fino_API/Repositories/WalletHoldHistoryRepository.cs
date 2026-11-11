using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class WalletHoldHistoryRepository : IWalletHoldHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public WalletHoldHistoryRepository(ApplicationDbContext context) => _context = context;

        public async Task<int> Insert(WalletHoldHistory entity)
        {
            await _context.WalletHoldHistories.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Hold history for the report: newest first, filtered and paged in the query. Returns the
        /// total the filter matched alongside the page, so the screen can page without a second call.
        /// </summary>
        public async Task<(int count, List<WalletHoldHistory> list)> Search(
            int userId, int walletId, DateTime? fromDate, DateTime? toDate, string action,
            IEnumerable<int>? allowedUserIds, int skip, int take)
        {
            var query = _context.WalletHoldHistories.AsNoTracking().Where(x => !x.IsDeleted);

            if (userId > 0) query = query.Where(x => x.UserID == userId);
            if (walletId > 0) query = query.Where(x => x.WalletID == walletId);
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
                .ThenByDescending(x => x.WalletHoldHistoryID)
                .Skip(Math.Max(skip, 0))
                .Take(take > 0 ? take : 25)
                .ToListAsync();

            return (count, list);
        }
    }
}
