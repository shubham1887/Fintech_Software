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
        /// <summary>Whose statement line this is — firm name and mobile, for the admin reading many users' rows.</summary>
        public string UserName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;

        /// <summary>
        /// Which wallet the money moved in. A statement that does not say this is unreadable the
        /// moment a user holds more than one wallet — a recharge debit and an AEPS credit sit next
        /// to each other with no way to tell them apart.
        /// </summary>
        public string WalletName { get; set; } = string.Empty;

        public string DOCType { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;

        /// <summary>The number the money went to — recharge mobile, account number, and so on.</summary>
        public string ConsumerNumber { get; set; } = string.Empty;

        /// <summary>
        /// Who the money moved to or from. On a fund transfer this is the other party; on a
        /// transaction it is the provider that served it. Blank for a purely internal posting.
        /// </summary>
        public string Counterparty { get; set; } = string.Empty;

        public string AmountCreditDebit { get; set; } = string.Empty;

        /// <summary>
        /// The OPERATOR's reference for the transaction — what a customer quotes when they call.
        /// This used to be the internal transaction id printed twice, once here and once in
        /// <see cref="ReferenceID"/>, so neither column told anyone anything they could use.
        /// </summary>
        public string ReferenceNo { get; set; } = string.Empty;

        /// <summary>Our own transaction id — the one to search this system by.</summary>
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

        /// <summary>
        /// The figures strip above the table — opening, closing, what settled, commission, charge
        /// and the net movement. Label → figure, the same shape every other report's strip uses,
        /// so the report screen needs no per-report code; keys are printed as given.
        /// </summary>
        public Dictionary<string, object> HeaderSummary { get; set; } = new();
    }
}
