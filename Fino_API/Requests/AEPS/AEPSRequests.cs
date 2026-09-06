namespace FintechApp.Requests.AEPS
{
    public class AEPSTransactionStatusRequest
    {
        public string OrderID { get; set; } = string.Empty;
    }

    public class SelectPIDOptionRequest
    {
        public string DeviceType { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string ActiveAEPS { get; set; } = string.Empty;
    }
}
