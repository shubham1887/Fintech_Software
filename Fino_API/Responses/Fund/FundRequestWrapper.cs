namespace FintechApp.Responses.Fund
{
    public class FundRequestWrapper
    {
        public int Count { get; set; }
        public List<FundRequestViewModel> list { get; set; } = new();
        public FundHeaderSummary HeaderSummary { get; set; } = new();
    }

    public class FundHeaderSummary
    {
        public decimal TotalAmount { get; set; }
        public decimal TotalCashCollection { get; set; }
        public int TotalCount { get; set; }
    }
}
