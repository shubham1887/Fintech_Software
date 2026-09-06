using FintechApp.Responses.TransactionMaster;

namespace FintechApp.Responses.TransactionProcessing
{
    public class ProviderWisePerformanceViewModel
    {
        public int ProviderID { get; set; }
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int PendingCount { get; set; }
        public int RefundCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessAmount { get; set; }
        public double SuccessRate { get; set; }
    }

    public class TxnListWrapper
    {
        public int Count { get; set; }
        public List<TransactionResponse> data { get; set; } = new();
        public string ReportPath { get; set; } = string.Empty;
    }

    // Processing-time distribution: how many successful txns completed within ProcTime seconds (60 = 60+).
    public class HourlyPerformanceViewModel
    {
        public double ProcTime { get; set; }
        public int Count { get; set; }
    }

    // Average processing time grouped into 2-hour intervals.
    public class Last12hrPerformanceViewModel
    {
        public string IntervalStart { get; set; } = string.Empty;
        public string IntervalEnd { get; set; } = string.Empty;
        public double AvgProcTime { get; set; }
        public int Count { get; set; }
    }
}
