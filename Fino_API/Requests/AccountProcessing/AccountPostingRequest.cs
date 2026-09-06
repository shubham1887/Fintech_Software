namespace FintechApp.Requests.AccountProcessing
{
    // Parameters for a single wallet ledger posting (debit or credit), ported from the
    // old AccountProcessing.DebitBalance/CreditBalance signatures.
    public class AccountPostingRequest
    {
        public int UserID { get; set; }
        public int WalletID { get; set; }

        // Net amount applied to the wallet balance (commission-inclusive). Always supplied as a
        // positive magnitude; the service negates it for a debit.
        public decimal AmountWithCommission { get; set; }

        // The face/transaction amount recorded on the ledger row (informational).
        public decimal Amount { get; set; }

        public string ConsumerNumber { get; set; } = string.Empty;
        public string CommissionCreditDebit { get; set; } = StaticModels.CommissionCreditDebitType.None;
        public string DOCType { get; set; } = string.Empty;
        public int AssignedUserID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public string TransactionDescription { get; set; } = string.Empty;
        public int ReferenceID { get; set; }
        public decimal TDS { get; set; }
        public decimal GST { get; set; }
        public decimal DiscountAmount { get; set; }
        public string PayMode { get; set; } = string.Empty;

        // Written back by AccountProcessingService on a successful posting: the balances the
        // ledger row was actually written with, read under the (UserID, WalletID) lock. A caller
        // that records its own row must use these — re-reading the wallet itself races with
        // concurrent postings and yields an opening balance that never matches the ledger.
        public decimal PostedOpeningBalance { get; set; }
        public decimal PostedClosingBalance { get; set; }
    }
}
