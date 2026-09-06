namespace FintechApp.Requests.Fund
{
    public class FundTransferReportRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public string ReportFor { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
        public int SkipCount { get; set; }
        public int TakeCount { get; set; } = 20;
        public string Search { get; set; } = string.Empty;
        public string ExportType { get; set; } = string.Empty;
        public string SortColumnName { get; set; } = string.Empty;
        public string SortDirection { get; set; } = string.Empty;
        public string CriteriaText { get; set; } = string.Empty;
        public bool IsExcelDownload { get; set; }
        public bool IsPDFDownload { get; set; }
    }
}
