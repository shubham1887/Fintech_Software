using FintechApp.Entities;

namespace FintechApp.Helpers
{
    /// <summary>
    /// Derives a wallet's opening and closing balance for one calendar day from the
    /// TransactionDetails ledger.
    ///
    /// Why the ledger and not UserBalanceHistory: every money movement — principal debit, commission
    /// credit, surcharge, fund transfer, refund — goes through AccountProcessing, which writes a
    /// TransactionDetails row stamped with the balance before and after that movement. It is the only
    /// complete record. UserBalanceHistory, by contrast, is written solely by an explicit API call
    /// that nothing in the running system makes, so reading an opening balance from it returned 0 for
    /// almost every account and date — which is what made Today's Summary look wrong.
    ///
    /// It also fixes the date: the old code always reported the wallet's CURRENT balance as the
    /// closing balance, so picking last Tuesday showed today's money against last Tuesday's activity.
    /// </summary>
    public static class LedgerBalances
    {
        /// <param name="Opening">Balance before the day's first movement.</param>
        /// <param name="Closing">Balance after the day's last movement.</param>
        /// <param name="FromLedger">
        /// False when the wallet has no ledger history at or before this date, i.e. the figures are a
        /// fallback rather than a reconstruction. Callers can use it to avoid presenting a guess as a
        /// reconciled number.
        /// </param>
        public readonly record struct DayBalance(decimal Opening, decimal Closing, bool FromLedger);

        /// <summary>
        /// Opening/closing for <paramref name="date"/> from this wallet's ledger rows.
        /// </summary>
        /// <param name="walletEntries">Ledger rows for ONE user + ONE wallet (any date range).</param>
        /// <param name="date">The day being reported (time part ignored).</param>
        /// <param name="liveBalance">The wallet's balance right now — used only for today, and as the
        /// fallback for a wallet that has no ledger history at all.</param>
        public static DayBalance ForDate(IEnumerable<TransactionDetails> walletEntries, DateTime date, decimal liveBalance)
        {
            DateTime day = date.Date;
            bool isToday = day == DateTime.Now.Date;

            var ordered = walletEntries
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.DateTime)
                .ThenBy(e => e.TransactionDetailID)
                .ToList();

            var onDay = ordered.Where(e => e.DateTime.Date == day).ToList();
            var before = ordered.Where(e => e.DateTime.Date < day).ToList();

            decimal opening;
            bool fromLedger = true;
            if (onDay.Count > 0)
                opening = onDay[0].OpeningBalance;                 // balance before the day's first movement
            else if (before.Count > 0)
                opening = before[^1].ClosingBalance;               // nothing moved today — carry yesterday forward
            else if (ordered.Count > 0)
                opening = ordered[0].OpeningBalance;               // the account's first movement is still in the future
            else
            {
                // No ledger history at all (a brand-new wallet, or one whose history predates the
                // ledger). Today's figure is the live balance; a past date cannot be reconstructed.
                opening = isToday ? liveBalance : 0m;
                fromLedger = false;
            }

            // For today the wallet itself is the truth — it is what the user can spend right now, and
            // it also catches any movement posted between the last ledger row and this read.
            decimal closing = isToday
                ? liveBalance
                : onDay.Count > 0 ? onDay[^1].ClosingBalance : opening;

            return new DayBalance(opening, closing, fromLedger);
        }
    }
}
