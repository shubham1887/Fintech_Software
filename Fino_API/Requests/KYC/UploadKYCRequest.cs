namespace FintechApp.Requests.KYC
{
    public class UploadKYCRequest
    {
        public int KycDocumentMasterID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public int SelectedUserID { get; set; }
        public KYCDocumentModel Document { get; set; } = new();
    }

    public class KYCDocumentModel
    {
        public string Data { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
