namespace Fino.NexmintApi.Responses
{
    /// <summary>
    /// Result of a Nexmint call.
    ///
    /// Three outcomes have to stay separate here, because money depends on telling them apart:
    ///   <see cref="Retryable"/> — the call never got an answer (network, timeout, HTTP 5xx). The
    ///     payout may or may not exist upstream, so the caller must leave it PENDING and ask again.
    ///     Treating this as failure is how a transfer gets refunded locally and paid anyway.
    ///   <see cref="Success"/> false — an answer arrived that was not a Nexmint envelope at all.
    ///   <see cref="ApiSuccess"/> false — a proper envelope saying no (HTTP 400/401/403). The
    ///     request was rejected before any money moved, and <see cref="Message"/> says why.
    ///
    /// On a clean accept, <see cref="Status"/> carries the provider's own vocabulary
    /// (pending / success / failed) — which on initiation is ALWAYS pending.
    /// </summary>
    public class NexmintApiResponse
    {
        public bool Success { get; set; }
        public bool Retryable { get; set; }
        public bool ApiSuccess { get; set; }
        public int HttpStatus { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty; // raw provider JSON, kept for the log

        // Parsed fields from data{} (best-effort; empty/zero when absent).
        public string Status { get; set; } = string.Empty; // SUCCESS / FAILED / PENDING (upper-cased)
        public string TransactionID { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string UtrNumber { get; set; } = string.Empty;

        // Set only on a failed transfer: Nexmint refunds our wallet itself, and these say how far
        // that has got ("pending" in progress, "success" complete).
        public string RefundStatus { get; set; } = string.Empty;
        public string RefundTransactionID { get; set; } = string.Empty;

        // Wallet enquiry.
        public decimal Balance { get; set; }      // spendable
        public decimal HoldBalance { get; set; }  // locked against in-flight payouts

        /// <summary>No answer — leave the transfer pending and enquire again.</summary>
        public static NexmintApiResponse Unreachable(string message) =>
            new() { Success = false, Retryable = true, Message = message };

        /// <summary>A definite no, before any money moved.</summary>
        public static NexmintApiResponse Fail(string message) =>
            new() { Success = false, Retryable = false, Message = message };
    }
}
