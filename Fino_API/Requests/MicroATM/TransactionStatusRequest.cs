namespace FintechApp.Requests.MicroATM
{
    public class TransactionStatusRequest
    {
        public string OrderID { get; set; } = string.Empty;
        public string TransactionID { get; set; } = string.Empty;
    }
}
