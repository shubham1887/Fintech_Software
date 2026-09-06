namespace FintechApp.Requests.CashCollection
{
    public class SelectCashCollectionRequest
    {
        public int UserID { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public int TakeCount { get; set; }
        public int SkipCount { get; set; }
        public bool IsExcelDownload { get; set; }
    }
}
