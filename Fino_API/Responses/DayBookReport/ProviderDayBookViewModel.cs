namespace FintechApp.Responses.DayBookReport
{
    public class ProviderDayBookViewModel
    {
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string Operator { get; set; } = string.Empty;
        public int TotalHits { get; set; }
        public decimal TotalAmount { get; set; }
        public int SuccessHits { get; set; }
        public decimal SuccessAmount { get; set; }
        public int FailedHits { get; set; }
        public decimal FailedAmount { get; set; }
        public int PendingHits { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal AdminCommission { get; set; }
        public decimal RetailerCommission { get; set; }
        public decimal DistributerCommission { get; set; }
        public decimal SuperDistributerCommission { get; set; }
        public decimal ProviderCommission { get; set; }
        public decimal Profit { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ProviderDayBookWrapper
    {
        public int Count { get; set; }
        public bool IsExcelDownload { get; set; }
        public List<ProviderDayBookViewModel> list { get; set; } = new();
    }
}
