namespace FintechApp.Requests.TransactionMaster
{
    public class GetTransactionRequest
    {
        public int TransactionID { get; set; }
        public int UserID { get; set; }
        public string TxnID { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
