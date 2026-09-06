namespace FintechApp.Responses.DMTSender
{
    public class DMTSenderResponse
    {
        public int SenderID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int UserID { get; set; }
        public bool IsActive { get; set; }
        public bool IsOTPVerified { get; set; }
        public bool IsKYC { get; set; }
        public string KYCDocumentURL { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
