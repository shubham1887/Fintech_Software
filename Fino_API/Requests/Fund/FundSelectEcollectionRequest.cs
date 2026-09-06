namespace FintechApp.Requests.Fund
{
    public class FundSelectEcollectionRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public int UserID { get; set; }
        public bool IsExcelDownload { get; set; }
    }
}
