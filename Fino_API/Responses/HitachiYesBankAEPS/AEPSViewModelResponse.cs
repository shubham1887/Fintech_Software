namespace FintechApp.Responses.HitachiYesBankAEPS
{
    public class AEPSViewModelResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string BankRRN { get; set; } = string.Empty;
        public string AvailableBalance { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AadhaarNumber { get; set; } = string.Empty;
        public string ConsumerNumber { get; set; } = string.Empty;
        public string APIUserRequestID { get; set; } = string.Empty;
        public string TransactionID { get; set; } = string.Empty;
        public string Stan { get; set; } = string.Empty;
        public int VendorID { get; set; }
        public string Amount { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public string DOC { get; set; } = string.Empty;
        public string AgentID { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public List<AEPSMinistatement> ministatements { get; set; } = new();
        public List<string> ministatementStr { get; set; } = new();
    }

    public class AEPSMinistatement
    {
        public string Date { get; set; } = string.Empty;
        public string TxnType { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Narration { get; set; } = string.Empty;
    }
}
