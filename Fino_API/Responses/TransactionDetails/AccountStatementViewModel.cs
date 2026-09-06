namespace FintechApp.Responses.TransactionDetails
{
    /// <summary>
    /// One row of the Account Statement / Admin Accounting report. Property names match the web
    /// report column keys exactly so they bind directly.
    /// </summary>
    public class AccountStatementViewModel
    {
        public int TransactionDetailID { get; set; }
        public DateTime DOC { get; set; }
        public string AliasName { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string DOCType { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string AmountCreditDebit { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public int ReferenceID { get; set; }
        public decimal Amount { get; set; }
        public decimal Charge { get; set; }
        public decimal GST { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TDS { get; set; }
        public decimal NetCommission { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string TransactionDescription { get; set; } = string.Empty;
    }

    public class AccountStatementWrapper
    {
        public int Count { get; set; }
        public List<AccountStatementViewModel> list { get; set; } = new();
    }
}
