namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Delivers a transaction's final result to the API user's configured callback URL.
    ///
    /// Called from every path that can settle a transaction — the synchronous /api/v1/recharge
    /// reply, an explicit status check, the provider push callback and the background
    /// status-check sweep — so the integrator is notified exactly once per settlement no matter
    /// how it settled.
    ///
    /// It takes only a transaction id on purpose: <see cref="IApiUserCallbackSender"/> re-reads
    /// the transaction and builds the payload through <see cref="IApiUserResponseBuilder"/>, so
    /// the callback body is the same contract as the initial reply and can never drift from it
    /// or carry stale data.
    ///
    /// This call only ENQUEUES — it returns immediately and the delivery (with its retries)
    /// happens on the background sender, so a slow or unreachable receiver never adds latency
    /// to the transaction that triggered it. Best-effort throughout: every failure is swallowed
    /// and logged; a callback problem must never affect the transaction or the caller.
    /// </summary>
    public interface ICallbackDispatchService
    {
        /// <summary>
        /// Push the settled result of one transaction. No-ops when the transaction is not an
        /// API-user transaction, is not in a final state, or the user has no callback URL for
        /// that service.
        /// </summary>
        Task DispatchForTransaction(int transactionMasterID);
    }
}
