namespace FintechApp.Requests.CashCollection
{
    public class AddCashCollectionRequest
    {
        public int ReceiverID { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
        public string UTRNumber { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
    }
}
