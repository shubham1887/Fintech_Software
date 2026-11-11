using FintechApp.StaticModels;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The ONE way an outstanding balance ever moves.
    ///
    /// Credit extended on a fund request, credit extended on a direct transfer, a reversal pulling
    /// the funds back, cash collected at the counter and an admin adjustment all land here. That is
    /// deliberate: the balance, the statement row behind it and the message to the person who owes
    /// the money have to happen together or not at all, and the only way to guarantee that is to
    /// have a single place that does all three.
    /// </summary>
    public interface IOutstandingService
    {
        /// <summary>Is the cash-collection feature switched on for this install?</summary>
        Task<bool> IsEnabled();

        /// <summary>The user's current outstanding, or zero when they have no record yet.</summary>
        Task<decimal> GetOutstanding(int userID);

        /// <summary>
        /// The user's outstanding and the credit limit IN FORCE — which is their own limit when one
        /// has been set for them, and the Key Setting default otherwise. <c>isDefaultLimit</c> says
        /// which, so a screen can show "using the network default" rather than implying somebody
        /// chose this number for this account.
        /// </summary>
        Task<(decimal outstanding, decimal maxOutstanding, bool isDefaultLimit)> GetStanding(int userID);

        /// <summary>
        /// The network-wide default credit limit (Key Setting <c>DefaultCreditLimit</c>), or zero
        /// when it is switched off. Zero means no credit for anyone who has not been given a limit
        /// of their own.
        /// </summary>
        Task<decimal> GetDefaultCreditLimit();

        /// <summary>
        /// Drops an account's own limit so the network default applies to it again.
        ///
        /// The counterpart of <see cref="SetCreditLimit"/>: without it, setting a limit once is
        /// irreversible — an account could never be put back on the default except by guessing
        /// what the default currently is and typing that.
        /// </summary>
        Task<string> ClearCreditLimit(int userID);

        /// <summary>
        /// Would a credit of <paramref name="amount"/> be accepted right now?
        ///
        /// A courtesy check, run BEFORE any money moves so the person raising the request is told
        /// "your credit limit is ₹2,00,000 and ₹1,80,000 is already outstanding" instead of having
        /// a transfer fail halfway. It is NOT the guard — the guard is inside
        /// <see cref="PostCredit"/>, where it is atomic. Returns <see cref="StatusType.Ok"/> when
        /// there is room.
        /// </summary>
        Task<string> CheckCreditAllowed(int userID, decimal amount);

        /// <summary>
        /// Increases <paramref name="userID"/>'s outstanding — funds taken on credit.
        ///
        /// Enforces the credit limit atomically and refuses rather than overshooting it. On success
        /// the statement row is written and the user is messaged. A failure means NOTHING changed,
        /// so a caller that has already moved money must undo that move.
        /// </summary>
        Task<(string failureMessage, OutstandingDeltaResult result)> PostCredit(OutstandingPosting posting);

        /// <summary>
        /// Reduces <paramref name="userID"/>'s outstanding — a payment collected, or a reversal
        /// taking the funds back out of their wallet.
        ///
        /// Refuses to collect more than is owed (which would make the balance an advance payment,
        /// something nothing downstream represents). On success the statement row is written and
        /// the user is messaged.
        /// </summary>
        Task<(string failureMessage, OutstandingDeltaResult result)> PostDebit(OutstandingPosting posting);

        /// <summary>
        /// Sets a user's credit limit. Admin-only by policy; the limit is what decides whether the
        /// Credit payment mode is offered to them at all.
        /// </summary>
        Task<string> SetCreditLimit(int userID, decimal maxOutstanding);

        /// <summary>
        /// Sends the credit/debit message for a posting that was made with
        /// <see cref="OutstandingPosting.SuppressMessage"/> set.
        ///
        /// This exists so a caller can post first and announce later. The fund flows have to post
        /// the debt BEFORE moving the money (the credit limit is enforced inside the posting, so a
        /// refusal must be able to happen while nothing has changed) — but a text saying "credit
        /// given, you now owe ₹3,00,000" must not go out until the funds have actually landed, or
        /// a wallet move that fails a moment later leaves the person holding a message about money
        /// they never received. The wording still lives in one place; only the timing moves.
        ///
        /// Never throws.
        /// </summary>
        Task Announce(OutstandingPosting posting, bool isCredit, decimal amount, decimal closing);

        /// <summary>
        /// Records which fund transaction a statement row belongs to, once that id is known.
        ///
        /// A direct transfer has to post the debt before it writes its own row (the credit limit
        /// is checked inside the posting), so the id does not exist yet at that moment. Without
        /// this the statement would show a credit line with no way back to the transfer — its
        /// mode, receipt and UTR — which is precisely the "when did I take it, how did I take it"
        /// question the statement is there to answer. Best-effort: a failure leaves the line
        /// readable, just not linked.
        /// </summary>
        Task LinkFundTransaction(OutstandingPosting posting, int fundTransactionID);

        /// <summary>
        /// Records the funded wallet's opening and closing balance against the statement row that
        /// was just written, once the money has actually moved.
        ///
        /// The statement's own opening/closing describe the DEBT; these describe the WALLET, which
        /// is what somebody reconciles a credit line against. Best-effort — a failure costs two
        /// columns on one row, never the posting.
        /// </summary>
        Task RecordWalletBalances(OutstandingPosting posting, decimal walletOpening, decimal walletClosing);
    }

    /// <summary>
    /// Everything one movement of an outstanding balance needs to be posted and explained.
    ///
    /// A parameter object rather than a long argument list because every field here ends up on a
    /// statement the person reads — and a statement row whose remark, mode or counterparty was
    /// left at a default is the row nobody can account for six months later.
    /// </summary>
    public class OutstandingPosting
    {
        /// <summary>Who owes the money — whose balance moves.</summary>
        public int UserID { get; set; }

        /// <summary>
        /// The counterparty: who extended the credit, or who received the payment.
        ///
        /// Outstanding is owed along one edge of the network (admin → super-distributor,
        /// distributor → retailer), so this is what gives a statement row its other side.
        /// </summary>
        public int ParentID { get; set; }

        /// <summary>Always positive. The direction comes from which method is called.</summary>
        public decimal Amount { get; set; }

        /// <summary>One of the <see cref="OutstandingLedger"/> type constants.</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>What the person will read on the statement line.</summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>How the money moved, for a collection (Cash, Bank, UPI…).</summary>
        public string PayMode { get; set; } = string.Empty;

        /// <summary>Bank reference for a collection paid by transfer.</summary>
        public string UTRNumber { get; set; } = string.Empty;

        /// <summary>The fund request / transfer this came from, when it came from one.</summary>
        public int? FundTransactionID { get; set; }

        /// <summary>Which wallet the funds landed in, when this is credit on a fund movement.</summary>
        public int? WalletID { get; set; }

        /// <summary>
        /// Set to skip the credit-limit guard. Only an admin adjustment does this — writing off a
        /// debt or correcting an opening balance must not be blocked by the limit it is fixing.
        /// </summary>
        public bool BypassLimit { get; set; }

        /// <summary>
        /// Filled IN by the service with the id of the statement row it wrote.
        ///
        /// It is here so a caller that only learns the fund-transaction id afterwards — a direct
        /// transfer records the debt before the transfer row exists — can still tie the two
        /// together with <see cref="IOutstandingService.LinkFundTransaction"/>, instead of leaving
        /// a statement line that cannot be traced back to the movement that caused it.
        /// </summary>
        public int LedgerRowID { get; set; }

        /// <summary>
        /// Set to skip messaging the user about this row.
        ///
        /// Used where a message has already gone out for the same event and a second one would
        /// read as a duplicate movement — the reversal flows already tell the user their wallet
        /// was debited, and they do not need a separate text a second later saying the matching
        /// debt went away.
        /// </summary>
        public bool SuppressMessage { get; set; }
    }
}
