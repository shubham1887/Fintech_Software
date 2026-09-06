namespace FintechApp.Responses.Transaction
{
    public class BankTransferResponse
    {
        public string Status { get; set; } = string.Empty; // SUCCESS / FAILED / PENDING
        public string Message { get; set; } = string.Empty;
        public int TransactionMasterID { get; set; }
        public string Kind { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string ProviderRefNo { get; set; } = string.Empty;
        public string DateTime { get; set; } = string.Empty;
    }
}
