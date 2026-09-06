namespace Fino.JaiJuiApi.Requests
{
    // Inputs for a JaiJui account/penny verify (IsVerify=true). AuthCode, registered Mobile and
    // ApiToken are resolved dynamically from configuration; everything here is per-request.
    public class AccountVerifyRequest
    {
        public string Mobile { get; set; } = string.Empty;           // sender mobile no
        public string SenderID { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "IMPS";            // IMPS / NEFT
        public string SenderName { get; set; } = string.Empty;
        public string APIUserRequestID { get; set; } = string.Empty; // unique id
    }
}
