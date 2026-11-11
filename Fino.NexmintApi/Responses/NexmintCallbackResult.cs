namespace Fino.NexmintApi.Responses
{
    /// <summary>
    /// A payout webhook Nexmint pushed at us, once its signature has been checked.
    ///
    /// <see cref="Verified"/> false means the push did not prove it came from Nexmint and must be
    /// discarded without touching a transaction — a callback is an instruction to settle or refund
    /// money, so an unsigned one is not "probably fine", it is an attacker's free payout.
    /// </summary>
    public class NexmintCallbackResult
    {
        public bool Verified { get; set; }
        public string Message { get; set; } = string.Empty;

        public string OrderID { get; set; } = string.Empty;       // our ProviderRequestID
        public string TransactionID { get; set; } = string.Empty; // Nexmint's id
        public string Status { get; set; } = string.Empty;        // SUCCESS / FAILED (upper-cased)
        public string UtrNumber { get; set; } = string.Empty;     // bank reference, success only
        public decimal Amount { get; set; }
        public string RefundStatus { get; set; } = string.Empty;
        public string RefundTransactionID { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;   // the provider's own wording

        public static NexmintCallbackResult Reject(string message) => new() { Message = message };
    }
}
