namespace FintechApp.Requests.KYC
{
    public class ApproveKYCRequest
    {
        public int UserID { get; set; }
        public bool IsApproved { get; set; }
        public string Remark { get; set; } = string.Empty;
        public int UserDocumentID { get; set; }
    }
}
