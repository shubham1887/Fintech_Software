namespace Fino.JaiJuiApi.Requests
{
    // Inputs for a JaiJui money transfer (IsVerify=false). AuthCode, registered Mobile and
    // ApiToken are resolved dynamically from configuration; everything here is per-transaction.
    public class MoneyTransferRequest
    {
        public string Mobile { get; set; } = string.Empty;           // sender mobile no
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string BeneficiaryID { get; set; } = string.Empty;    // from ViewBeneficiaryList
        public string SenderID { get; set; } = string.Empty;         // from ViewBeneficiaryList
        public string PaymentType { get; set; } = "IMPS";            // IMPS / NEFT
        public decimal Amount { get; set; }                          // Min 10, Max 5000
        public string Name { get; set; } = string.Empty;            // beneficiary name
        public string APIUserRequestID { get; set; } = string.Empty; // unique id (our txn ref)
    }
}
