namespace FintechApp.Requests.Transaction
{
    // Distinguishes the money-out flows that share the same internal pipeline.
    public enum TransferKind
    {
        DMT = 1,        // Domestic Money Transfer (MoneyTransfer)
        Payout = 2,     // Bank payout
        VendorPay = 3   // Vendor payment
    }

    public class BankTransferRequest
    {
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public decimal Amount { get; set; }
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string SenderID { get; set; } = string.Empty;
        public string BeneficiaryID { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty; // NEFT / IMPS / UPI
        public string Remark { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string APIUserRequestID { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
    }
}
