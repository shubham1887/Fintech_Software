namespace Fino.Pay1Api.Responses
{
    // Pay1's "Auto Status Update Url" push, verified and mapped into the fields the host needs to
    // settle a transaction. Verified is false when the signature does not match — the host must
    // then ignore the push entirely, never settle on it.
    public class Pay1CallbackResult
    {
        public bool Verified { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>Our own request id (client_req_id) — the key back to the transaction.</summary>
        public string RequestID { get; set; } = string.Empty;

        /// <summary>Pay1's 10-digit transaction id.</summary>
        public string TransactionID { get; set; } = string.Empty;

        /// <summary>The biller / operator reference, when Pay1 has one.</summary>
        public string OperatorID { get; set; } = string.Empty;

        /// <summary>success / failure, as sent.</summary>
        public string Status { get; set; } = string.Empty;

        public string ErrorCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PartnerID { get; set; } = string.Empty;
        public string ClosingBalance { get; set; } = string.Empty;

        /// <summary>True when Pay1 is reversing a transaction it had already confirmed
        /// (reversal_status = 1) and has credited the money back to us.</summary>
        public bool IsReversal { get; set; }

        /// <summary>Everything Pay1 posted, for the callback log.</summary>
        public string Raw { get; set; } = string.Empty;

        public bool IsSuccess => Status.Equals("success", StringComparison.OrdinalIgnoreCase);
        public bool IsFailure => Status.StartsWith("fail", StringComparison.OrdinalIgnoreCase);
    }
}
