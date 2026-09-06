namespace FintechApp.Responses.CashCollection
{
    public class OutstandingWrapper
    {
        public List<OutstandingViewModel> data { get; set; } = new();
        public bool IsExcelDownload { get; set; }
        public string ReportPath { get; set; } = string.Empty;
    }
}
