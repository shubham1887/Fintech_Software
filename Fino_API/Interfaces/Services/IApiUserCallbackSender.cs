namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Performs the actual callback HTTP delivery for one settled transaction — resolve the URL,
    /// build the contract payload, sign it, POST it, retry, and log the attempt trail.
    ///
    /// Separated from <see cref="ICallbackDispatchService"/> so that the request path only ever
    /// enqueues (instant) while this — which can block for tens of seconds retrying a slow
    /// receiver — runs on the background sender.
    /// </summary>
    public interface IApiUserCallbackSender
    {
        /// <summary>
        /// Deliver the settled result of one transaction. No-ops when the transaction is not an
        /// API-user transaction, is not in a final state, or the user has no callback URL for
        /// that service. Never throws.
        /// </summary>
        Task Send(int transactionMasterID);
    }
}
