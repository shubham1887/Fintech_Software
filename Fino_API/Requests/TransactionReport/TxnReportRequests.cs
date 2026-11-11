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
        /// <summary>
        /// Complaint state to narrow to - ACCEPT / REJECT / PROCESS, or ALL (and blank) for no
        /// filter. The report screens have offered this dropdown all along, but the field was
        /// missing from this class, so the value was dropped at deserialisation and the filter
        /// silently did nothing on every report that showed it.
        /// </summary>
        public string Complaint { get; set; } = string.Empty;
        /// <summary>
        /// Wallet to narrow to. Several report screens offer the dropdown; the ones whose
        /// endpoint reads it honour it, and this is where the value arrives.
        /// </summary>
        public int WalletID { get; set; }
        /// <summary>Aggregation granularity for grouped reports (e.g. Purchase Report): "Month" or "Date".</summary>
        public string GroupBy { get; set; } = string.Empty;

        /// <summary>Effective page size — TakeCount (web UI convention) wins over PageSize.</summary>
        public int EffectiveTake => TakeCount > 0 ? TakeCount : PageSize;
        /// <summary>Effective skip — SkipCount wins over (PageNumber-1)*PageSize.</summary>
        public int EffectiveSkip => TakeCount > 0 ? Math.Max(0, SkipCount) : Math.Max(0, (PageNumber - 1) * PageSize);

        /// <summary>
        /// How many rows to read for THIS request, for the stores that treat a take of 0 as "use
        /// your own default page" rather than as "no limit".
        ///
        /// That default is 50 on the filter reports, and an export asking for everything with a
        /// take of 0 was handed exactly those 50 rows of a 322-row report. A window the caller
        /// named is still honoured on an export, because that is how a large one is collected —
        /// window after window — and only a download that named none is read as "all of it".
        /// </summary>
        public int ReadTake => EffectiveTake > 0 ? EffectiveTake : (IsExcelDownload ? int.MaxValue : 0);
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
