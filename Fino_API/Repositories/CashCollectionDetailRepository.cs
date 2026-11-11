using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    /// <summary>
    /// The outstanding statement (one row per movement). The legacy whole-table reads are cached
    /// and kept for existing callers; the statement screen and the exports go through
    /// <see cref="GetStatement"/>, which filters, orders and pages in SQL — this table only ever
    /// grows, so the cached shape could not stay the answer.
    /// </summary>
    public class CashCollectionDetailRepository : ICashCollectionDetailRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CashCollectionDetail>> _listCache;
        public CashCollectionDetailRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CashCollectionDetail>>(); }
        public async Task<List<CashCollectionDetail>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCashCollectionDetails, () => _context.CashCollectionDetails.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<CashCollectionDetail>();
        public async Task<CashCollectionDetail?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CashCollectionDetailID == id); }
        public async Task<List<CashCollectionDetail>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(CashCollectionDetail entity) { await _context.CashCollectionDetails.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionDetails); return result; }
        public async Task<int> Update(CashCollectionDetail entity) { _context.CashCollectionDetails.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionDetails); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CashCollectionDetails.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionDetails); return result; }

        /// <summary>
        /// The shared filter, so the page and its count can never disagree about what they are
        /// counting — the classic way a pager ends up offering a page that comes back empty.
        /// </summary>
        private IQueryable<CashCollectionDetail> Filtered(
            int? userID, int? parentID, DateTime? fromUtc, DateTime? toUtc)
        {
            var q = _context.CashCollectionDetails.AsNoTracking()
                .Where(x => !x.IsDeleted && !x.IsDelete);

            if (userID is > 0) q = q.Where(x => x.UserID == userID.Value);
            if (parentID is > 0) q = q.Where(x => x.ParentID == parentID.Value);
            if (fromUtc.HasValue) q = q.Where(x => x.DateTime >= fromUtc.Value);
            if (toUtc.HasValue) q = q.Where(x => x.DateTime <= toUtc.Value);

            return q;
        }

        public async Task<List<CashCollectionDetail>> GetStatement(
            int? userID, int? parentID, DateTime? fromUtc, DateTime? toUtc, int skip, int take)
        {
            // A statement is read newest-first, and the id breaks ties: two postings in the same
            // millisecond must still come back in the order they were written, or the
            // opening/closing chain on screen appears to jump.
            var q = Filtered(userID, parentID, fromUtc, toUtc)
                .OrderByDescending(x => x.DateTime)
                .ThenByDescending(x => x.CashCollectionDetailID);

            if (skip > 0) q = (IOrderedQueryable<CashCollectionDetail>)q.Skip(skip);

            // An unbounded take is how one wide date range turns into the whole table in memory.
            int pageSize = take is > 0 and <= 5000 ? take : 200;
            return await q.Take(pageSize).ToListAsync();
        }

        public async Task<int> CountStatement(int? userID, int? parentID, DateTime? fromUtc, DateTime? toUtc)
            => await Filtered(userID, parentID, fromUtc, toUtc).CountAsync();

        public async Task<List<CashCollectionDetail>> GetMovementsForAgeing(IReadOnlyCollection<int>? userIDs, int maxRows)
        {
            var q = _context.CashCollectionDetails.AsNoTracking()
                .Where(x => !x.IsDeleted && !x.IsDelete);

            // An empty set means "nobody", not "everybody" — a caller whose downline is empty must
            // get an empty ageing, never the whole network's.
            if (userIDs != null)
            {
                if (userIDs.Count == 0) return new List<CashCollectionDetail>();
                var ids = userIDs.ToList();
                q = q.Where(x => ids.Contains(x.UserID));
            }

            // Oldest first: FIFO matching consumes the earliest credit with the earliest payment,
            // and the id breaks ties so same-millisecond postings age in the order written.
            return await q
                .OrderBy(x => x.DateTime)
                .ThenBy(x => x.CashCollectionDetailID)
                .Take(maxRows is > 0 and <= 500_000 ? maxRows : 200_000)
                .ToListAsync();
        }

        public async Task<int> SetWalletBalances(int cashCollectionDetailID, decimal walletOpening, decimal walletClosing)
        {
            int updated = await _context.CashCollectionDetails
                .Where(x => x.CashCollectionDetailID == cashCollectionDetailID)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.WalletOpening, walletOpening)
                    .SetProperty(x => x.WalletClosing, walletClosing));

            if (updated > 0) _listCache.Remove(CacheKeys.AllCashCollectionDetails);
            return updated;
        }

        public async Task<int> SetFundTransactionID(int cashCollectionDetailID, int fundTransactionID)
        {
            int updated = await _context.CashCollectionDetails
                .Where(x => x.CashCollectionDetailID == cashCollectionDetailID)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.FundTransactionID, fundTransactionID));

            if (updated > 0) _listCache.Remove(CacheKeys.AllCashCollectionDetails);
            return updated;
        }
    }
}
