namespace FintechApp.Responses.Ledger
{
    public class LedgerWrapper
    {
        public List<LedgerResponse> list { get; set; } = new();
        public List<LedgerHeader> Headerlist { get; set; } = new();
        public string ReportPath { get; set; } = string.Empty;
        public bool IsExcelDownload { get; set; }
        public bool IsPDFDownload { get; set; }
        public int Count { get; set; }
    }
}
