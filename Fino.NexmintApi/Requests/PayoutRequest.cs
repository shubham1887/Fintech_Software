namespace Fino.NexmintApi.Requests
{
    // Inputs for a Nexmint payout (POST /fund-transfer). client_id and client_secret are resolved
    // dynamically from the provider row or configuration; everything here is per-transaction.
    public class PayoutRequest
    {
        public string OrderID { get; set; } = string.Empty;           // our unique ref, max 100 chars
        public string PaymentMethod { get; set; } = "IMPS";           // IMPS / NEFT / RTGS
        public string AccountHolderName { get; set; } = string.Empty; // letters and spaces only
        public string BankName { get; set; } = string.Empty;          // letters and spaces only
        public string AccountNumber { get; set; } = string.Empty;     // 5-18 digits
        public string IFSC { get; set; } = string.Empty;              // ABCD0123456
        public string Mobile { get; set; } = string.Empty;            // 10 digits
        public decimal Amount { get; set; }                           // min 1; decimals are truncated upstream
        public string AccountType { get; set; } = "saving";           // saving / current
        public string Remark { get; set; } = string.Empty;            // 1-250 chars
    }
}
