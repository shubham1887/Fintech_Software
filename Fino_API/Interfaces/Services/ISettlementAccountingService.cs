using FintechApp.Entities;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The bridge between a business event that has already moved money and the two places that
    /// have to know about it: the tax stamp on the transaction, and the ledger outbox.
    ///
    /// Everything here is CALLED AFTER the money moved, so nothing here may throw and nothing here
    /// may change the caller's outcome. A failure to stamp or to enqueue is logged and swallowed —
    /// the same contract AccountCoreProcessingService.NotifyOutcome already works to, and for the
    /// same reason: a settlement that has debited a wallet must not be undone because a bookkeeping
    /// side-effect had a bad day. The cost of that choice is that a missed enqueue is invisible
    /// here, which is exactly what the daily integrity checks exist to catch.
    ///
    /// Why a service of its own rather than lines inside each caller: the enqueue has to choose its
    /// event from the resolved billing basis, and that decision must read the same way for recharge,
    /// DMT, a status-check sweep and a manual settle. Four copies of it would be four chances to
    /// post a gross sale for a commission service.
    /// </summary>
    public interface ISettlementAccountingService
    {
        /// <summary>
        /// A transaction has settled to Success. Resolves the tax as of the transaction's own date,
        /// stamps <see cref="TransactionTaxDetail"/> (once — a re-settle does not duplicate it), and
        /// enqueues the ledger events the resolved basis calls for.
        /// </summary>
        Task OnTransactionSettled(int transactionMasterID);

        /// <summary>
        /// A transaction was refunded — it failed, and every leg of it was reversed. The charge GST
        /// breakdown is cleared back to zero, because a failed transaction earns nothing and owes
        /// no tax on nothing: a provider does not charge GST on a transfer that did not happen, and
        /// a retailer was given their commission back along with the principal.
        ///
        /// The tax STAMP is deliberately left alone. It records what was resolved when the sale was
        /// made, the reversal is booked as its own event, and silently deleting the stamp would
        /// leave a credit note with nothing to point at.
        /// </summary>
        Task OnTransactionRefunded(int transactionMasterID);

        /// <summary>
        /// Stamp the charge/GST breakdown on a transaction that settled BEFORE the breakdown
        /// existed, or before its band had a GST setting. Returns true when this call wrote one.
        ///
        /// Same writer as a live settlement — deliberately, because a backfill that computed the
        /// split its own way would be a second copy of the formula and would eventually disagree
        /// with the first. It only ever touches a SUCCESS that carries no stamp: an already-stamped
        /// transaction keeps the figures it settled on.
        /// </summary>
        Task<bool> StampChargeGstForSettledTransaction(int transactionMasterID);

        /// <summary>
        /// The admin injected float into the system (VirtualBalance). This is the one wallet credit
        /// that is real money arriving from outside: Dr Bank, Cr User Wallet control. A transfer
        /// between two users is NOT this — both sides sit inside the wallet control account and net
        /// to nothing, so it posts nothing at all.
        /// </summary>
        Task OnWalletToppedUp(int virtualBalanceID, int walletID, decimal amount, DateTime when);

        /// <summary>We bought stock from a provider: Dr Provider Stock, Cr Bank.</summary>
        Task OnProviderPurchase(int providerPurchaseID, int providerID, decimal amount, DateTime when);
    }
}
