namespace FintechApp.Responses.SubMode
{
    public class SubModeResponse
    {
        public int SubModeID { get; set; }
        public int ModeID { get; set; }
        public string ModeName { get; set; } = string.Empty;
        public string SubModeName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public string QRImage { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
