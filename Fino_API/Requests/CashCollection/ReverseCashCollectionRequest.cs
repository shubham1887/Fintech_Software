namespace FintechApp.Requests.CashCollection
{
    public class ReverseCashCollectionRequest
    {
        public int ReceiverID { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
    }
}
