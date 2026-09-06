namespace Fino.JaiJuiApi.Responses
{
    // Result of a JaiJui DMT call.
    //   Success  = transport succeeded and the body parsed (NOT the money-transfer outcome).
    //   Status   = the provider's own outcome vocabulary: SUCCESS / FAILURE / PROCESS.
    //   Data     = the raw provider JSON (kept for logging / pass-through).
    // The provider returns HTTP 200 even for a declined transfer, so callers must branch on Status,
    // not on Success alone.
    public class JaiJuiApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;

        // Parsed provider fields (best-effort; empty/zero when absent).
        public string Status { get; set; } = string.Empty; // SUCCESS / FAILURE / PROCESS
        public string APIUserRequestID { get; set; } = string.Empty;
        public string TransactionID { get; set; } = string.Empty;
        public string VendorID { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal Amount { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal Balance { get; set; } // provider wallet balance (balance enquiry)
        public decimal Commission { get; set; }
        public string BeneficiaryName { get; set; } = string.Empty;
        public string ConsumerNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsOTP { get; set; }

        public static JaiJuiApiResponse Fail(string message) => new() { Success = false, Message = message };
    }
}
