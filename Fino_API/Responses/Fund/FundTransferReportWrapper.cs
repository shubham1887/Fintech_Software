namespace FintechApp.Responses.Fund
{
    public class FundTransferReportWrapper
    {
        public int Count { get; set; }
        public List<FundRequestViewModel> data { get; set; } = new();
        public FundHeaderSummary HeaderViewModel { get; set; } = new();
        public string ReportPath { get; set; } = string.Empty;
        public bool IsDownload { get; set; }
    }
}
