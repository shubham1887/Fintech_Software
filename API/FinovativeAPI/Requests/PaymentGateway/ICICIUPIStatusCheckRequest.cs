namespace Project.Requests.PaymentGateway
{
    public class ICICIUPIStatusCheckRequest
    {
        public string merchantId { get; set; }
        public string subMerchantId { get; set; }
        public string terminalId { get; set; }
        public string merchantTranId { get; set; }
    }
}