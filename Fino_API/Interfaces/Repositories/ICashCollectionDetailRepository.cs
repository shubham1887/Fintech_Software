using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ICashCollectionDetailRepository
    {
        /// <summary>
        /// The whole statement table, cached in process.
        ///
        /// Kept for the existing callers, but NOT for the statement screen: this table grows by a
        /// row for every credit and every collection on the network, for ever. Holding all of it
        /// in memory and filtering in LINQ works on a new install and stops working on a busy one.
        /// Anything that needs a user's statement should use <see cref="GetStatement"/>, which
        /// filters and pages in the database.
        /// </summary>
        Task<List<CashCollectionDetail>> GetAll();
        Task<CashCollectionDetail?> GetByID(int id);
        Task<List<CashCollectionDetail>> GetByUserID(int userID);
        Task<int> Insert(CashCollectionDetail entity);
        Task<int> Update(CashCollectionDetail entity);
        Task<int> Delete(int id);

        /// <summary>
        /// One page of a statement, filtered and ordered in the database, newest first.
        ///
        /// <paramref name="userID"/> narrows to whose debt it is; <paramref name="parentID"/> to
        /// who it is owed to — outstanding is per edge of the network, so a distributor looking at
        /// a retailer wants only the rows between the two of them, not that retailer's debt to
        /// anyone else. A null for either means "any".
        /// </summary>
        Task<List<CashCollectionDetail>> GetStatement(
            int? userID, int? parentID, DateTime? fromUtc, DateTime? toUtc, int skip, int take);

        /// <summary>
        /// How many rows that same filter matches, so the client can page and an export knows when
        /// it has collected everything.
        /// </summary>
        Task<int> CountStatement(int? userID, int? parentID, DateTime? fromUtc, DateTime? toUtc);

        /// <summary>
        /// Every live movement for the given users, oldest first — the input to a FIFO ageing.
        ///
        /// Separate from <see cref="GetAll"/> because ageing needs the rows in order and only for
        /// the accounts the caller may see, and separate from <see cref="GetStatement"/> because
        /// it spans users rather than paging one of them. Ordered in SQL (date, then id) so two
        /// postings in the same millisecond age in the order they were written.
        ///
        /// A null or empty <paramref name="userIDs"/> means every user — which is what admin asks
        /// for. <paramref name="maxRows"/> is a hard ceiling: this table only grows, and an
        /// ageing that silently turns into a full table scan is a server that falls over on the
        /// one install big enough to need the report.
        /// </summary>
        Task<List<CashCollectionDetail>> GetMovementsForAgeing(IReadOnlyCollection<int>? userIDs, int maxRows);

        /// <summary>
        /// Ties a statement row to the fund transaction that caused it, once that id is known.
        /// Writes that one column only, so it can never disturb the amounts on the row.
        /// </summary>
        Task<int> SetFundTransactionID(int cashCollectionDetailID, int fundTransactionID);

        /// <summary>
        /// Records what the funded wallet went from and to for this movement.
        ///
        /// Separate from the insert because the debt is posted BEFORE the money moves (the credit
        /// limit is enforced in that posting), so the wallet's closing balance does not exist yet
        /// at the moment the row is written. Writes those two columns only.
        /// </summary>
        Task<int> SetWalletBalances(int cashCollectionDetailID, decimal walletOpening, decimal walletClosing);
    }
}
