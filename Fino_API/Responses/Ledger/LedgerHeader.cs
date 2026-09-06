namespace FintechApp.Responses.Ledger
{
    public class LedgerHeader
    {
        public decimal OpeningBal { get; set; }
        public decimal TransferAmount { get; set; }
        public decimal ReceiverAmount { get; set; }
        public decimal ReverseAmountCr { get; set; }
        public decimal ReverseAmountDr { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal TDS { get; set; }
        public decimal Surcharge { get; set; }
        public decimal Requested { get; set; }
        public decimal Debited { get; set; }
        public decimal ComplaintAmount { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal ExpectedCloseBal { get; set; }
        public decimal Difference { get; set; }
        public decimal PastFailAmount { get; set; }
        public decimal PastSuccessAmount { get; set; }
        public decimal PGAmount { get; set; }
        public decimal ECollectionAmount { get; set; }
        public decimal AEPSAmount { get; set; }
        public decimal MATMAmount { get; set; }
    }
}
