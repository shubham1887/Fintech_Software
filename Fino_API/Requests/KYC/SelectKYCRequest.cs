namespace FintechApp.Requests.KYC
{
    public class SelectKYCRequest
    {
        public string Status { get; set; } = string.Empty;
        public bool IsExcelDownload { get; set; }
        public string Search { get; set; } = string.Empty;
        public int TakeCount { get; set; }
        public int SkipCount { get; set; }
    }

    public class SelectDocByUserRequest
    {
        public int UserID { get; set; }
    }

    public class SelectByKYCIDRequest
    {
        public int UserDocumentID { get; set; }
    }
}
