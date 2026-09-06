namespace FintechApp.Responses.Ledger
{
    public class LedgerResponse
    {
        public int TransactionDetailID { get; set; }
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string AssignedUserName { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal Amount { get; set; }
        public decimal TDS { get; set; }
        public int WalletID { get; set; }
        public string WalletName { get; set; } = string.Empty;
        public string ConsumerNumber { get; set; } = string.Empty;
        public string ReferenceID { get; set; } = string.Empty;
        public string TransactionID { get; set; } = string.Empty;
        public string RechargeDate { get; set; } = string.Empty;
        public string DOCType { get; set; } = string.Empty;
        public decimal ClosingBalance { get; set; }
        public decimal CommissionAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DOC { get; set; } = string.Empty;
        public CustomAmount customAmount { get; set; } = new();
        public string AmountCreditDebit { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }

    public class CustomAmount
    {
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
