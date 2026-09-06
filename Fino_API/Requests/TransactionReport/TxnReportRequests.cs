namespace FintechApp.Requests.TransactionReport
{
    public class TxnReportRequest
    {
        public int UserID { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public bool IsExcelDownload { get; set; }
        public string Search { get; set; } = string.Empty;
        // Filter-report fields (the web UI's TxnReportScreen sends these).
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }
        public int TakeCount { get; set; }
        public int SkipCount { get; set; }
        public string Criteria { get; set; } = string.Empty;
        public string CriteriaText { get; set; } = string.Empty;
        /// <summary>Aggregation granularity for grouped reports (e.g. Purchase Report): "Month" or "Date".</summary>
        public string GroupBy { get; set; } = string.Empty;

        /// <summary>Effective page size — TakeCount (web UI convention) wins over PageSize.</summary>
        public int EffectiveTake => TakeCount > 0 ? TakeCount : PageSize;
        /// <summary>Effective skip — SkipCount wins over (PageNumber-1)*PageSize.</summary>
        public int EffectiveSkip => TakeCount > 0 ? Math.Max(0, SkipCount) : Math.Max(0, (PageNumber - 1) * PageSize);
    }

    public class TxnReportByServiceRequest
    {
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int TransactionMasterID { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public bool IsExcelDownload { get; set; }
    }
}
