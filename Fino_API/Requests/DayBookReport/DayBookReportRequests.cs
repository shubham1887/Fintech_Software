namespace FintechApp.Requests.DayBookReport
{
    public class ProviderDayBookRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public int UserID { get; set; }
        public bool IsExcelDownload { get; set; }
    }
}
